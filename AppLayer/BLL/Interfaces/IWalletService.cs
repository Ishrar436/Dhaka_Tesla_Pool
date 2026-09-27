using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetBalanceAsync(Guid userId);
        Task TopUpAsync(Guid userId, long amountPaisa);
        Task DebitAsync(Guid userId, long amountPaisa, Guid? rideRequestId);
    }
}
