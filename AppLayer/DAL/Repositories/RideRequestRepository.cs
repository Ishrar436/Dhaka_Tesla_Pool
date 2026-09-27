using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class RideRequestRepository : GenericRepository<RideRequest>, IRideRequestRepository
{
    public RideRequestRepository(TeslaDbContext context) : base(context) { }

    public async Task<IEnumerable<RideRequest>> GetHistoryForPassengerAsync(Guid passengerId) =>
        await _dbSet.Where(r => r.PassengerId == passengerId)
                     .OrderByDescending(r => r.RequestedAt)
                     .ToListAsync();

    public async Task<RideRequest?> GetWithDetailsAsync(Guid rideRequestId) =>
        await _dbSet.Include(r => r.PickupZone)
                     .Include(r => r.DropoffZone)
                     .Include(r => r.Pool)
                     .FirstOrDefaultAsync(r => r.Id == rideRequestId);

    public async Task<IEnumerable<RideRequest>> GetByPoolIdAsync(Guid poolId) =>
        await _dbSet.Where(r => r.PoolId == poolId).ToListAsync();
}