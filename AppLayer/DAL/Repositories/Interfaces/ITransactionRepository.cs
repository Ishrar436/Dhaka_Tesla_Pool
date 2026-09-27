using System;
using System.Collections.Generic;
using System.Text;
using DAL.EF.Tables;

namespace DAL.Repositories.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId);
    }
}
