using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
{
    public WalletRepository(TeslaDbContext context) : base(context) { }

    public async Task<Wallet?> GetByUserIdAsync(Guid userId) =>
        await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId);
}