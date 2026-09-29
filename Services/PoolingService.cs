using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;

namespace BLL.Services
{
    public class PoolingService : IPoolingService
    {
        private readonly IUnitOfWork _uow;

        public PoolingService(IUnitOfWork uow) => _uow = uow;

        public async Task<Pool> FindOrCreatePoolAsync(Guid pickupZoneId, int seatsRequested, Guid? excludeDriverId = null) =>
            await TryFindOrCreatePoolAsync(pickupZoneId, seatsRequested, excludeDriverId)
                ?? throw new InvalidOperationException("No seats or drivers are available for this pickup zone right now.");

        // Matching rule: a passenger joins a waiting pool that starts in the same pickup zone
        // and still has enough free seats. Once the driver accepts, the pool is closed to newcomers.
        public async Task<Pool?> TryFindOrCreatePoolAsync(Guid pickupZoneId, int seatsRequested, Guid? excludeDriverId = null)
        {
            var waitingPools = await _uow.Pools.GetWaitingPoolsAsync();

            foreach (var pool in waitingPools)
            {
                if (excludeDriverId.HasValue && pool.DriverId == excludeDriverId.Value) continue;

                var active = pool.RideRequests
                    .Where(r => r.Status is not ("Cancelled" or "Completed"))
                    .ToList();

                if (active.Count == 0) continue;
                if (active.Any(r => r.Status != "Waiting")) continue;
                if (active.Any(r => r.PickupZoneId != pickupZoneId)) continue;

                var vehicle = await _uow.Vehicles.GetByIdAsync(pool.VehicleId);
                if (vehicle == null) continue;

                if (active.Sum(r => r.SeatsRequested) + seatsRequested <= vehicle.Capacity)
                    return pool;
            }

            // No pool to join, so start one with an online driver who has no active pool.
            var onlineDrivers = await _uow.Drivers.GetOnlineDriversAsync();
            foreach (var driver in onlineDrivers)
            {
                if (excludeDriverId.HasValue && driver.Id == excludeDriverId.Value) continue;
                if ((await _uow.Pools.GetActivePoolsForDriverAsync(driver.Id)).Any()) continue;

                var vehicle = await _uow.Vehicles.GetActiveVehicleForDriverAsync(driver.Id);
                if (vehicle == null || vehicle.Capacity < seatsRequested) continue;

                var newPool = new Pool
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicle.Id,
                    DriverId = driver.Id,
                    Status = "Waiting",
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Pools.AddAsync(newPool);
                return newPool;
            }

            return null;
        }
    }
}
