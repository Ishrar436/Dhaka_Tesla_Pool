using DAL.EF;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TeslaDbContext _context;

    public IUserRepository Users { get; }
    public IDriverRepository Drivers { get; }
    public IVehicleRepository Vehicles { get; }
    public IZoneRepository Zones { get; }
    public IPoolRepository Pools { get; }
    public IRideRequestRepository RideRequests { get; }
    public IRideStatusHistoryRepository RideStatusHistories { get; }
    public IWalletRepository Wallets { get; }
    public ITransactionRepository Transactions { get; }

    public UnitOfWork(TeslaDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Drivers = new DriverRepository(_context);
        Vehicles = new VehicleRepository(_context);
        Zones = new ZoneRepository(_context);
        Pools = new PoolRepository(_context);
        RideRequests = new RideRequestRepository(_context);
        RideStatusHistories = new RideStatusHistoryRepository(_context);
        Wallets = new WalletRepository(_context);
        Transactions = new TransactionRepository(_context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync() =>
        await _context.Database.BeginTransactionAsync();

    public void Dispose() => _context.Dispose();
}