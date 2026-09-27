using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class RideRequestService : IRideRequestService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPoolingService _poolingService;
        private readonly IFareService _fareService;
        private readonly IRideStateMachineService _stateMachine;

        public RideRequestService(
            IUnitOfWork uow,
            IPoolingService poolingService,
            IFareService fareService,
            IRideStateMachineService stateMachine)
        {
            _uow = uow;
            _poolingService = poolingService;
            _fareService = fareService;
            _stateMachine = stateMachine;
        }

        public async Task<RideRequestDto> CreateRideRequestAsync(CreateRideRequestDto dto)
        {
            var pickupZone = await _uow.Zones.GetByIdAsync(dto.PickupZoneId)
                ?? throw new ArgumentException("Invalid pickup zone.");
            var dropoffZone = await _uow.Zones.GetByIdAsync(dto.DropoffZoneId)
                ?? throw new ArgumentException("Invalid dropoff zone.");

            var pool = await _poolingService.FindOrCreatePoolAsync(dto.PickupZoneId, dto.DropoffZoneId, dto.SeatsRequested);

            var existingRequestsInPool = await _uow.RideRequests.GetByPoolIdAsync(pool.Id);
            var currentOccupants = existingRequestsInPool.Count();

            var fare = _fareService.CalculateFare(pickupZone, dropoffZone, currentOccupants);

            var rideRequest = new RideRequest
            {
                Id = Guid.NewGuid(),
                PassengerId = dto.PassengerId,
                PickupZoneId = dto.PickupZoneId,
                DropoffZoneId = dto.DropoffZoneId,
                PoolId = pool.Id,
                SeatsRequested = dto.SeatsRequested,
                Status = "Waiting",
                EstimatedFarePaisa = fare.TotalFarePaisa,
                RequestedAt = DateTime.UtcNow
            };

            await _uow.RideRequests.AddAsync(rideRequest);
            await LogStatusChangeAsync(rideRequest.Id, null, "Waiting");
            await _uow.SaveChangesAsync();

            return MapToDto(rideRequest, pickupZone.Name, dropoffZone.Name);
        }

        public async Task<RideRequestDto?> GetStatusAsync(Guid rideRequestId)
        {
            var request = await _uow.RideRequests.GetWithDetailsAsync(rideRequestId);
            if (request == null) return null;

            return MapToDto(request, request.PickupZone.Name, request.DropoffZone.Name);
        }

        public async Task<IEnumerable<RideRequestDto>> GetHistoryAsync(Guid passengerId)
        {
            var requests = await _uow.RideRequests.GetHistoryForPassengerAsync(passengerId);
            var result = new List<RideRequestDto>();

            foreach (var r in requests)
            {
                var pickup = await _uow.Zones.GetByIdAsync(r.PickupZoneId);
                var dropoff = await _uow.Zones.GetByIdAsync(r.DropoffZoneId);
                result.Add(MapToDto(r, pickup?.Name ?? "", dropoff?.Name ?? ""));
            }

            return result;
        }

        public async Task CancelAsync(Guid rideRequestId)
        {
            var request = await _uow.RideRequests.GetByIdAsync(rideRequestId)
                ?? throw new ArgumentException("Ride request not found.");

            _stateMachine.ValidateTransition(request.Status, "Cancelled");

            var oldStatus = request.Status;
            request.Status = "Cancelled";
            request.CompletedAt = DateTime.UtcNow;

            _uow.RideRequests.Update(request);
            await LogStatusChangeAsync(rideRequestId, oldStatus, "Cancelled");
            await _uow.SaveChangesAsync();
        }

        private async Task LogStatusChangeAsync(Guid rideRequestId, string? fromStatus, string toStatus)
        {
            await _uow.RideStatusHistories.AddAsync(new RideStatusHistory
            {
                Id = Guid.NewGuid(),
                RideRequestId = rideRequestId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ChangedAt = DateTime.UtcNow
            });
        }

        private static RideRequestDto MapToDto(RideRequest r, string pickupZoneName, string dropoffZoneName) => new()
        {
            Id = r.Id,
            PassengerId = r.PassengerId,
            PickupZoneName = pickupZoneName,
            DropoffZoneName = dropoffZoneName,
            PoolId = r.PoolId,
            Status = r.Status,
            EstimatedFarePaisa = r.EstimatedFarePaisa,
            FinalFarePaisa = r.FinalFarePaisa,
            RequestedAt = r.RequestedAt
        };
    }
}
