using BLL.DTOs;
using BLL.Exceptions;
using BLL.Helpers;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class PoolLifecycleService : IPoolLifecycleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRideStateMachineService _stateMachine;

        public PoolLifecycleService(IUnitOfWork uow, IRideStateMachineService stateMachine)
        {
            _uow = uow;
            _stateMachine = stateMachine;
        }

        public async Task<IEnumerable<PoolDto>> GetMyPoolsAsync(Guid driverUserId)
        {
            var driver = await GetDriverAsync(driverUserId);
            var pools = await _uow.Pools.GetActivePoolsForDriverAsync(driver.Id);

            var result = new List<PoolDto>();
            foreach (var pool in pools) result.Add(await MapAsync(pool));
            return result;
        }

        public async Task<PoolDto> GetPoolAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);
            return await MapAsync(pool);
        }

        public async Task AcceptAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "Waiting")
                throw new InvalidOperationException($"This pool is {pool.Status} and can't be accepted.");

            var waiting = pool.RideRequests.Where(r => r.Status == "Waiting").ToList();
            if (waiting.Count == 0)
                throw new InvalidOperationException("There are no waiting passengers to accept.");

            foreach (var ride in waiting)
                await MoveAsync(ride, "Matched");

            await _uow.SaveChangesAsync();
        }

        public async Task StartAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "Waiting")
                throw new InvalidOperationException($"This pool is {pool.Status} and can't be started.");

            var active = ActiveRides(pool);
            if (active.Count == 0)
                throw new InvalidOperationException("There are no passengers in this pool.");
            if (active.Any(r => r.Status == "Waiting"))
                throw new InvalidOperationException("Accept the waiting passengers before starting the trip.");

            foreach (var ride in active)
                await MoveAsync(ride, "InProgress");

            pool.Status = "InProgress";
            pool.StartedAt = DateTime.UtcNow;
            _uow.Pools.Update(pool);

            await _uow.SaveChangesAsync();
        }

        public async Task CompleteAsync(Guid driverUserId, Guid poolId)
        {
            var (driver, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "InProgress")
                throw new InvalidOperationException("Only a trip that is in progress can be completed.");

            foreach (var ride in pool.RideRequests.Where(r => r.Status == "InProgress").ToList())
            {
                ride.FinalFarePaisa = ride.EstimatedFarePaisa;
                await MoveAsync(ride, "Completed");
                await SettleFareAsync(ride, driver);
            }

            pool.Status = "Completed";
            pool.CompletedAt = DateTime.UtcNow;
            _uow.Pools.Update(pool);

            // One SaveChanges, so statuses, fares and wallet changes commit together or not at all.
            await _uow.SaveChangesAsync();
        }

        private async Task<Driver> GetDriverAsync(Guid userId) =>
            await _uow.Drivers.GetByUserIdAsync(userId)
                ?? throw new ForbiddenException("Only drivers can manage pools.");

        private async Task<(Driver driver, Pool pool)> LoadOwnedPoolAsync(Guid userId, Guid poolId)
        {
            var driver = await GetDriverAsync(userId);
            var pool = await _uow.Pools.GetWithRideRequestsAsync(poolId)
                ?? throw new NotFoundException("Pool not found.");

            if (pool.DriverId != driver.Id)
                throw new ForbiddenException("This pool belongs to another driver.");

            return (driver, pool);
        }

        private static List<RideRequest> ActiveRides(Pool pool) =>
            pool.RideRequests.Where(r => r.Status is not ("Cancelled" or "Completed")).ToList();

        private async Task MoveAsync(RideRequest ride, string toStatus)
        {
            _stateMachine.ValidateTransition(ride.Status, toStatus);

            var from = ride.Status;
            ride.Status = toStatus;
            if (toStatus is "Completed" or "Cancelled") ride.CompletedAt = DateTime.UtcNow;

            _uow.RideRequests.Update(ride);
            await RideHistoryLog.AddAsync(_uow, ride.Id, from, toStatus);
        }

        // Wallet first: if the passenger's TeslaPay balance covers the fare it moves to the driver.
        // If not, the fare is treated as paid in cash and no wallet changes.
        private async Task SettleFareAsync(RideRequest ride, Driver driver)
        {
            var fare = ride.FinalFarePaisa ?? ride.EstimatedFarePaisa;
            var passengerWallet = await _uow.Wallets.GetByUserIdAsync(ride.PassengerId);
            var driverWallet = await _uow.Wallets.GetByUserIdAsync(driver.UserId);

            if (passengerWallet == null || driverWallet == null || passengerWallet.BalancePaisa < fare)
                return;

            passengerWallet.BalancePaisa -= fare;
            driverWallet.BalancePaisa += fare;
            _uow.Wallets.Update(passengerWallet);
            _uow.Wallets.Update(driverWallet);

            var now = DateTime.UtcNow;
            await _uow.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = passengerWallet.Id,
                RideRequestId = ride.Id,
                AmountPaisa = fare,
                Type = "Debit",
                CreatedAt = now
            });
            await _uow.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = driverWallet.Id,
                RideRequestId = ride.Id,
                AmountPaisa = fare,
                Type = "Credit",
                CreatedAt = now
            });
        }

        private async Task<PoolDto> MapAsync(Pool pool)
        {
            var vehicle = await _uow.Vehicles.GetByIdAsync(pool.VehicleId);
            var passengers = new List<PoolPassengerDto>();

            foreach (var r in pool.RideRequests.OrderBy(r => r.RequestedAt))
            {
                var user = await _uow.Users.GetByIdAsync(r.PassengerId);
                var pickup = await _uow.Zones.GetByIdAsync(r.PickupZoneId);
                var dropoff = await _uow.Zones.GetByIdAsync(r.DropoffZoneId);

                passengers.Add(new PoolPassengerDto
                {
                    RideRequestId = r.Id,
                    PassengerName = user?.Name ?? "",
                    Seats = r.SeatsRequested,
                    PickupZoneName = pickup?.Name ?? "",
                    DropoffZoneName = dropoff?.Name ?? "",
                    Status = r.Status,
                    FarePaisa = r.FinalFarePaisa ?? r.EstimatedFarePaisa
                });
            }

            return new PoolDto
            {
                Id = pool.Id,
                Status = pool.Status,
                VehicleName = vehicle?.Name ?? "",
                Capacity = vehicle?.Capacity ?? 0,
                SeatsTaken = ActiveRides(pool).Sum(r => r.SeatsRequested),
                Passengers = passengers
            };
        }
    }
}
