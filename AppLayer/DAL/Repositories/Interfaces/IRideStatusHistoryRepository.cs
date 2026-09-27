using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IRideStatusHistoryRepository : IGenericRepository<RideStatusHistory>
    {
        Task<IEnumerable<RideStatusHistory>> GetByRideRequestIdAsync(Guid rideRequestId);
    }
}
