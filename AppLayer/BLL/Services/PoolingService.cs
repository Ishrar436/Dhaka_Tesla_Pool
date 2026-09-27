using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class PoolingService : IPoolingService
    {
        private readonly IUnitOfWork _uow;

        public PoolingService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // Assumption (PRD Section 4): "compatible route" = same pickup zone AND same dropoff zone.
        // Runs inside a DB transaction and re-checks capacity right before committing,
        // so two concurrent requests can't both squeeze into the last seat.
        public async Task<Pool> FindOrCreatePoolAsync(Guid pickupZoneId, Guid dropoffZoneId, int seatsRequested)
        {
            await using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                var candidatePools = (await _uow.Pools.GetAllAsync())
                    .Where(p => p.Status == "Waiting")
                    .ToList();

                foreach (var pool in candidatePools)
                {
                    var requestsInPool = (await _uow.RideRequests.GetByPoolIdAsync(pool.Id)).ToList();

                    var sameRoute = requestsInPool.Any() &&
                        requestsInPool.All(r => r.PickupZoneId == pickupZoneId && r.DropoffZoneId == dropoffZoneId);

                    if (!sameRoute) continue;

                    var vehicle = await _uow.Vehicles.GetByIdAsync(pool.VehicleId);
                    if (vehicle == null) continue;

                    var occupiedSeats = requestsInPool
                        .Where(r => r.Status is "Waiting" or "Matched" or "InProgress")
                        .Sum(r => r.SeatsRequested);

                    if (occupiedSeats + seatsRequested <= vehicle.Capacity)
                    {
                        await transaction.CommitAsync();
                        return pool;
                    }
                }

                // No compatible pool with room — find an online driver with an active vehicle to start one.
                var onlineDrivers = await _uow.Drivers.GetOnlineDriversAsync();
                var driver = onlineDrivers.FirstOrDefault();
                if (driver == null)
                    throw new InvalidOperationException("No online drivers available to start a pool.");

                var driverVehicle = await _uow.Vehicles.GetActiveVehicleForDriverAsync(driver.Id);
                if (driverVehicle == null || driverVehicle.Capacity < seatsRequested)
                    throw new InvalidOperationException("No vehicle with sufficient capacity available.");

                var newPool = new Pool
                {
                    Id = Guid.NewGuid(),
                    VehicleId = driverVehicle.Id,
                    DriverId = driver.Id,
                    Status = "Waiting",
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Pools.AddAsync(newPool);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
                return newPool;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Pool capacity changed concurrently — please retry the request.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
