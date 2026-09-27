using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(TeslaDbContext context) : base(context) { }

    public async Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId) =>
        await _dbSet.Where(v => v.DriverId == driverId).ToListAsync();

    public async Task<Vehicle?> GetActiveVehicleForDriverAsync(Guid driverId) =>
        await _dbSet.FirstOrDefaultAsync(v => v.DriverId == driverId && v.IsActive);
}