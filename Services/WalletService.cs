using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;

        public WalletService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<WalletDto> GetBalanceAsync(Guid userId)
        {
            var wallet = await _uow.Wallets.GetByUserIdAsync(userId)
                ?? throw new ArgumentException("Wallet not found.");

            return new WalletDto { UserId = userId, BalancePaisa = wallet.BalancePaisa };
        }

        public async Task TopUpAsync(Guid userId, long amountPaisa)
        {
            if (amountPaisa <= 0) throw new ArgumentException("Top-up amount must be positive.");

            var wallet = await _uow.Wallets.GetByUserIdAsync(userId)
                ?? throw new ArgumentException("Wallet not found.");

            wallet.BalancePaisa += amountPaisa;
            _uow.Wallets.Update(wallet);

            await _uow.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                AmountPaisa = amountPaisa,
                Type = "Credit",
                CreatedAt = DateTime.UtcNow
            });

            await _uow.SaveChangesAsync();
        }

        public async Task DebitAsync(Guid userId, long amountPaisa, Guid? rideRequestId)
        {
            var wallet = await _uow.Wallets.GetByUserIdAsync(userId)
                ?? throw new ArgumentException("Wallet not found.");

            if (wallet.BalancePaisa < amountPaisa)
                throw new InvalidOperationException("Insufficient wallet balance.");

            wallet.BalancePaisa -= amountPaisa;
            _uow.Wallets.Update(wallet);

            await _uow.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                RideRequestId = rideRequestId,
                AmountPaisa = amountPaisa,
                Type = "Debit",
                CreatedAt = DateTime.UtcNow
            });

            await _uow.SaveChangesAsync();
        }
    }
}
