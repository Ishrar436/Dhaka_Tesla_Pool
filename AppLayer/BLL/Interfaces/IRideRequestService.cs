using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IRideRequestService
    {
        Task<RideRequestDto> CreateRideRequestAsync(CreateRideRequestDto dto);
        Task<RideRequestDto?> GetStatusAsync(Guid rideRequestId);
        Task<IEnumerable<RideRequestDto>> GetHistoryAsync(Guid passengerId);
        Task CancelAsync(Guid rideRequestId);
    }
}
