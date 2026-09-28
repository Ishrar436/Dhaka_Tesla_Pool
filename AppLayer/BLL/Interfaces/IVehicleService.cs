using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleDto> RegisterVehicleAsync(Guid userId, CreateVehicleDto dto);
        Task<IEnumerable<VehicleDto>> GetMineAsync(Guid userId);
    }
}
