using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(TeslaDbContext context) : base(context) { }

    public async Task<User?> GetByPhoneAsync(string phone) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Phone == phone);

    public async Task<bool> PhoneExistsAsync(string phone) =>
        await _dbSet.AnyAsync(u => u.Phone == phone);
}