using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class PoolRepository : GenericRepository<Pool>, IPoolRepository
{
    public PoolRepository(TeslaDbContext context) : base(context) { }

    public async Task<Pool?> GetWithRideRequestsAsync(Guid poolId) =>
        await _dbSet.Include(p => p.RideRequests)
                     .FirstOrDefaultAsync(p => p.Id == poolId);

    // NOTE: for real concurrency safety, wrap the caller of this in a
    // transaction and re-check capacity before committing (see BLL notes below)
    public async Task<Pool?> FindOpenPoolForVehicleAsync(Guid vehicleId) =>
        await _dbSet.Where(p => p.VehicleId == vehicleId && p.Status == "Waiting")
                     .Include(p => p.RideRequests)
                     .FirstOrDefaultAsync();

    public async Task<IEnumerable<Pool>> GetActivePoolsForDriverAsync(Guid driverId) =>
        await _dbSet.Where(p => p.DriverId == driverId &&
                                 (p.Status == "Waiting" || p.Status == "InProgress"))
                     .Include(p => p.RideRequests)
                     .ToListAsync();
}