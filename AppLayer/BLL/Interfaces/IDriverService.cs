using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IDriverService
    {
        Task GoOnlineAsync(Guid userId);
        Task GoOfflineAsync(Guid userId);
        Task<DriverDto> GetProfileAsync(Guid userId);
    }
}
