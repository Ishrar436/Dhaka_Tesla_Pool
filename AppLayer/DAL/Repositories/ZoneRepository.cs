
using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ZoneRepository : GenericRepository<Zone>, IZoneRepository
{
    public ZoneRepository(TeslaDbContext context) : base(context) { }

    public async Task<Zone?> GetByNameAsync(string name) =>
        await _dbSet.FirstOrDefaultAsync(z => z.Name == name);
}