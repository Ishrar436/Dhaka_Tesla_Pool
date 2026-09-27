using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IPoolRepository : IGenericRepository<Pool>
    {
        Task<Pool?> GetWithRideRequestsAsync(Guid poolId);
        Task<Pool?> FindOpenPoolForVehicleAsync(Guid vehicleId);
        Task<IEnumerable<Pool>> GetActivePoolsForDriverAsync(Guid driverId);
    }
}
