using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IDriverService
    {
        Task GoOnlineAsync(Guid driverId);
        Task GoOfflineAsync(Guid driverId);
        Task<DriverDto?> GetProfileAsync(Guid driverId);
    }
}
