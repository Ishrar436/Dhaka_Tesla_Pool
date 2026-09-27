using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IDriverRepository Drivers { get; }
    IVehicleRepository Vehicles { get; }
    IZoneRepository Zones { get; }
    IPoolRepository Pools { get; }
    IRideRequestRepository RideRequests { get; }
    IRideStatusHistoryRepository RideStatusHistories { get; }
    IWalletRepository Wallets { get; }
    ITransactionRepository Transactions { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}