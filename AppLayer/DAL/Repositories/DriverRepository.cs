using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class DriverRepository : GenericRepository<Driver>, IDriverRepository
{
    public DriverRepository(TeslaDbContext context) : base(context) { }

    public async Task<Driver?> GetByUserIdAsync(Guid userId) =>
        await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);

    public async Task<Driver?> GetWithVehiclesAsync(Guid driverId) =>
        await _dbSet.Include(d => d.Vehicles).FirstOrDefaultAsync(d => d.Id == driverId);

    public async Task<IEnumerable<Driver>> GetOnlineDriversAsync() =>
        await _dbSet.Where(d => d.IsOnline).ToListAsync();
}