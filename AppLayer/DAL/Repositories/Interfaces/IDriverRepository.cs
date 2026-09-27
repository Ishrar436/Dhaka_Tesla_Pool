using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IDriverRepository : IGenericRepository<Driver>
    {
        Task<Driver?> GetByUserIdAsync(Guid userId);
        Task<Driver?> GetWithVehiclesAsync(Guid driverId);
        Task<IEnumerable<Driver>> GetOnlineDriversAsync();
    }
}
