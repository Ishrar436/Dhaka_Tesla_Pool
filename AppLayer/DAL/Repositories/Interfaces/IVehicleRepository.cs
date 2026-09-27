using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverId);
        Task<Vehicle?> GetActiveVehicleForDriverAsync(Guid driverId);
    }
}
