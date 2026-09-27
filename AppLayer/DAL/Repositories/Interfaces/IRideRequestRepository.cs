using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IRideRequestRepository : IGenericRepository<RideRequest>
    {
        Task<IEnumerable<RideRequest>> GetHistoryForPassengerAsync(Guid passengerId);
        Task<RideRequest?> GetWithDetailsAsync(Guid rideRequestId);
        Task<IEnumerable<RideRequest>> GetByPoolIdAsync(Guid poolId);
    }
}
