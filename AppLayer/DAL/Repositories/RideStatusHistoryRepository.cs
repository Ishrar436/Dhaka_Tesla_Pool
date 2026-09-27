using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class RideStatusHistoryRepository : GenericRepository<RideStatusHistory>, IRideStatusHistoryRepository
{
    public RideStatusHistoryRepository(TeslaDbContext context) : base(context) { }

    public async Task<IEnumerable<RideStatusHistory>> GetByRideRequestIdAsync(Guid rideRequestId) =>
        await _dbSet.Where(h => h.RideRequestId == rideRequestId)
                     .OrderBy(h => h.ChangedAt)
                     .ToListAsync();
}