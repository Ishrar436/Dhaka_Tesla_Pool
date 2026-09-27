using DAL.EF;
using DAL.EF.Tables;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(TeslaDbContext context) : base(context) { }

    public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId) =>
        await _dbSet.Where(t => t.WalletId == walletId)
                     .OrderByDescending(t => t.CreatedAt)
                     .ToListAsync();
}