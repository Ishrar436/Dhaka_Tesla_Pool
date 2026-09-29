using BLL.DTOs;
using BLL.Exceptions;
using BLL.Helpers;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;

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

        public async Task<RideRequestDto> CreateRideRequestAsync(Guid passengerId, CreateRideRequestDto dto)
        {
            if (dto.PickupZoneId == dto.DropoffZoneId)
                throw new ArgumentException("Pickup and dropoff zones must be different.");

            var pickupZone = await _uow.Zones.GetByIdAsync(dto.PickupZoneId)
                ?? throw new ArgumentException("Invalid pickup zone.");
            var dropoffZone = await _uow.Zones.GetByIdAsync(dto.DropoffZoneId)
                ?? throw new ArgumentException("Invalid dropoff zone.");

            // Everything from "pick a pool" to "save the request" happens in one transaction.
            // The app lock makes concurrent requests take turns, so seats can't be double-booked.
            await using var tx = await _uow.BeginTransactionAsync();
            await _uow.AcquireMatchingLockAsync();

            var pool = await _poolingService.FindOrCreatePoolAsync(dto.PickupZoneId, dto.SeatsRequested, null);

            var occupiedSeats = pool.RideRequests
                .Where(r => r.Status is not ("Cancelled" or "Completed"))
                .Sum(r => r.SeatsRequested);
            var fare = _fareService.CalculateFare(pickupZone, dropoffZone, dto.SeatsRequested, occupiedSeats);

            if (dto.PaymentMethod == "Wallet")
            {
                var wallet = await _uow.Wallets.GetByUserIdAsync(passengerId);
                if (wallet == null || wallet.BalancePaisa < fare.TotalFarePaisa)
                    throw new ArgumentException("Your TeslaPay balance doesn't cover this fare. Top up or choose cash.");
            }

            var ride = new RideRequest
            {
                Id = Guid.NewGuid(),
                PassengerId = passengerId,
                PickupZoneId = dto.PickupZoneId,
                DropoffZoneId = dto.DropoffZoneId,
                PoolId = pool.Id,
                SeatsRequested = dto.SeatsRequested,
                PaymentMethod = dto.PaymentMethod,
                Status = "Waiting",
                EstimatedFarePaisa = fare.TotalFarePaisa,
                RequestedAt = DateTime.UtcNow
            };

            await _uow.RideRequests.AddAsync(ride);
            await RideHistoryLog.AddAsync(_uow, ride.Id, null, "Waiting");
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();

            return MapToDto(ride, pickupZone.Name, dropoffZone.Name);
        }

        public async Task<RideRequestDto> GetStatusAsync(Guid passengerId, Guid rideRequestId)
        {
            var ride = await _uow.RideRequests.GetWithDetailsAsync(rideRequestId)
                ?? throw new NotFoundException("Ride request not found.");

            if (ride.PassengerId != passengerId)
                throw new ForbiddenException("This ride belongs to another passenger.");

            return MapToDto(ride, ride.PickupZone.Name, ride.DropoffZone.Name);
        }

        public async Task<IEnumerable<RideRequestDto>> GetHistoryAsync(Guid passengerId)
        {
            var rides = await _uow.RideRequests.GetHistoryForPassengerAsync(passengerId);
            var result = new List<RideRequestDto>();

            foreach (var r in rides)
            {
                var pickup = await _uow.Zones.GetByIdAsync(r.PickupZoneId);
                var dropoff = await _uow.Zones.GetByIdAsync(r.DropoffZoneId);
                result.Add(MapToDto(r, pickup?.Name ?? "", dropoff?.Name ?? ""));
            }

            return result;
        }

        // Full audit trail of one ride: every status change, when, and why. Passenger sees only their own.
        public async Task<IEnumerable<RideTimelineEntryDto>> GetTimelineAsync(Guid passengerId, Guid rideRequestId)
        {
            var ride = await _uow.RideRequests.GetByIdAsync(rideRequestId)
                ?? throw new NotFoundException("Ride request not found.");

            if (ride.PassengerId != passengerId)
                throw new ForbiddenException("This ride belongs to another passenger.");

            var entries = await _uow.RideStatusHistories.GetByRideRequestIdAsync(rideRequestId);
            return entries.Select(h => new RideTimelineEntryDto
            {
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedAt = h.ChangedAt,
                Reason = h.Reason
            }).ToList();
        }

        public async Task CancelAsync(Guid passengerId, Guid rideRequestId)
        {
            var ride = await _uow.RideRequests.GetByIdAsync(rideRequestId)
                ?? throw new NotFoundException("Ride request not found.");

            if (ride.PassengerId != passengerId)
                throw new ForbiddenException("This ride belongs to another passenger.");

            if (ride.Status == "InProgress")
                throw new InvalidOperationException("A ride that is already in progress can't be cancelled.");

            _stateMachine.ValidateTransition(ride.Status, "Cancelled");

            var oldStatus = ride.Status;
            ride.Status = "Cancelled";
            ride.CompletedAt = DateTime.UtcNow;
            _uow.RideRequests.Update(ride);
            await RideHistoryLog.AddAsync(_uow, ride.Id, oldStatus, "Cancelled", "Cancelled by the passenger");

            // If nobody is left in the pool, close it so the driver is free again.
            if (ride.PoolId != null)
            {
                var pool = await _uow.Pools.GetWithRideRequestsAsync(ride.PoolId.Value);
                if (pool != null && pool.RideRequests.All(r => r.Status is "Cancelled" or "Completed"))
                {
                    pool.Status = "Cancelled";
                    _uow.Pools.Update(pool);
                }
            }

            await _uow.SaveChangesAsync();
        }

        private static RideRequestDto MapToDto(RideRequest r, string pickupZoneName, string dropoffZoneName) => new()
        {
            Id = r.Id,
            PassengerId = r.PassengerId,
            PickupZoneName = pickupZoneName,
            DropoffZoneName = dropoffZoneName,
            PoolId = r.PoolId,
            Seats = r.SeatsRequested,
            PaymentMethod = r.PaymentMethod,
            Status = r.Status,
            EstimatedFarePaisa = r.EstimatedFarePaisa,
            FinalFarePaisa = r.FinalFarePaisa,
            RequestedAt = r.RequestedAt
        };
    }
}
