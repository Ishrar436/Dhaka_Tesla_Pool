using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IRideRequestService
    {
        Task<RideRequestDto> CreateRideRequestAsync(Guid passengerId, CreateRideRequestDto dto);
        Task<RideRequestDto> GetStatusAsync(Guid passengerId, Guid rideRequestId);
        Task<IEnumerable<RideRequestDto>> GetHistoryAsync(Guid passengerId);
        Task<IEnumerable<RideTimelineEntryDto>> GetTimelineAsync(Guid passengerId, Guid rideRequestId);
        Task CancelAsync(Guid passengerId, Guid rideRequestId);
    }
}
