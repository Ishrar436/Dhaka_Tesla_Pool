using DAL.EF.Tables;
using DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Helpers
{
    internal static class RideHistoryLog
    {
        public static Task AddAsync(IUnitOfWork uow, Guid rideRequestId, string? from, string to) =>
            uow.RideStatusHistories.AddAsync(new RideStatusHistory
            {
                Id = Guid.NewGuid(),
                RideRequestId = rideRequestId,
                FromStatus = from,
                ToStatus = to,
                ChangedAt = DateTime.UtcNow
            });
    }
}
