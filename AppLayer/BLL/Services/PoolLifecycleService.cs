using BLL.DTOs;
using BLL.Exceptions;
using BLL.Helpers;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;

namespace BLL.Services
{
    public class PoolLifecycleService : IPoolLifecycleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRideStateMachineService _stateMachine;
        private readonly IPoolingService _pooling;
        private readonly IFareService _fares;

        public PoolLifecycleService(
            IUnitOfWork uow,
            IRideStateMachineService stateMachine,
            IPoolingService pooling,
            IFareService fares)
        {
            _uow = uow;
            _stateMachine = stateMachine;
            _pooling = pooling;
            _fares = fares;
        }

        public async Task<IEnumerable<PoolDto>> GetMyPoolsAsync(Guid driverUserId)
        {
            var driver = await GetDriverAsync(driverUserId);
            var pools = await _uow.Pools.GetActivePoolsForDriverAsync(driver.Id);

            var result = new List<PoolDto>();
            foreach (var pool in pools) result.Add(await MapAsync(pool));
            return result;
        }

        // Finished pools (Completed or Cancelled), newest first.
        public async Task<IEnumerable<PoolDto>> GetHistoryAsync(Guid driverUserId)
        {
            var driver = await GetDriverAsync(driverUserId);
            var pools = await _uow.Pools.GetHistoryForDriverAsync(driver.Id);

            var result = new List<PoolDto>();
            foreach (var pool in pools) result.Add(await MapAsync(pool));
            return result;
        }

        public async Task<PoolDto> GetPoolAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);
            return await MapAsync(pool);
        }

        public async Task AcceptAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "Waiting")
                throw new InvalidOperationException($"This pool is {pool.Status} and can't be accepted.");

            var waiting = pool.RideRequests.Where(r => r.Status == "Waiting").ToList();
            if (waiting.Count == 0)
                throw new InvalidOperationException("There are no waiting passengers to accept.");

            foreach (var ride in waiting)
                await MoveAsync(ride, "Matched");

            await _uow.SaveChangesAsync();
        }

        // Driver says no to a pool he hasn't accepted yet. Each passenger is moved to another
        // driver's pool if one exists; otherwise that passenger's ride is cancelled with a reason.
        public async Task DeclineAsync(Guid driverUserId, Guid poolId)
        {
            // Same lock as ride requests: we're moving passengers between pools.
            await using var tx = await _uow.BeginTransactionAsync();
            await _uow.AcquireMatchingLockAsync();

            var (driver, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "Waiting")
                throw new InvalidOperationException($"This pool is {pool.Status} and can't be declined.");

            var active = ActiveRides(pool);
            if (active.Count == 0 || active.Any(r => r.Status != "Waiting"))
                throw new InvalidOperationException(
                    "Only a pool with unaccepted passengers can be declined. After accepting, use cancel.");

            foreach (var ride in active)
                await ReassignOrCancelAsync(ride, driver);

            pool.Status = "Cancelled";
            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
        }

        // Driver backs out after accepting but before the trip starts. Passengers are cancelled
        // (they were already promised a car, so silently swapping drivers would be worse) and re-request.
        public async Task CancelAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "Waiting")
                throw new InvalidOperationException("A pool can only be cancelled before the trip starts.");

            var active = ActiveRides(pool);
            if (active.Any(r => r.Status == "Waiting"))
                throw new InvalidOperationException("This pool hasn't been accepted yet. Use decline instead.");

            foreach (var ride in active)
                await MoveAsync(ride, "Cancelled", "Cancelled by the driver");

            pool.Status = "Cancelled";
            await _uow.SaveChangesAsync();
        }

        public async Task ArriveAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "Waiting")
                throw new InvalidOperationException("Arrival can only be marked before the trip starts.");

            var active = ActiveRides(pool);
            if (active.Any(r => r.Status == "Waiting"))
                throw new InvalidOperationException("Accept the waiting passengers before marking arrival.");

            var matched = active.Where(r => r.Status == "Matched").ToList();
            if (matched.Count == 0)
                throw new InvalidOperationException("There is no one to mark as arrived.");

            foreach (var ride in matched)
                await MoveAsync(ride, "DriverArrived");

            await _uow.SaveChangesAsync();
        }

        public async Task StartAsync(Guid driverUserId, Guid poolId)
        {
            var (_, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "Waiting")
                throw new InvalidOperationException($"This pool is {pool.Status} and can't be started.");

            var active = ActiveRides(pool);
            if (active.Any(r => r.Status != "DriverArrived"))
                throw new InvalidOperationException("Mark arrival before starting the trip.");
            if (active.Count == 0)
                throw new InvalidOperationException("There are no passengers in this pool.");
            if (active.Any(r => r.Status == "Waiting"))
                throw new InvalidOperationException("Accept the waiting passengers before starting the trip.");

            foreach (var ride in active)
                await MoveAsync(ride, "InProgress");

            pool.Status = "InProgress";
            pool.StartedAt = DateTime.UtcNow;
            _uow.Pools.Update(pool);

            await _uow.SaveChangesAsync();
        }

        public async Task CompleteAsync(Guid driverUserId, Guid poolId)
        {
            var (driver, pool) = await LoadOwnedPoolAsync(driverUserId, poolId);

            if (pool.Status != "InProgress")
                throw new InvalidOperationException("Only a trip that is in progress can be completed.");

            foreach (var ride in pool.RideRequests.Where(r => r.Status == "InProgress").ToList())
            {
                ride.FinalFarePaisa = ride.EstimatedFarePaisa;
                await MoveAsync(ride, "Completed");
                await SettleFareAsync(ride, driver);
            }

            pool.Status = "Completed";
            pool.CompletedAt = DateTime.UtcNow;
            _uow.Pools.Update(pool);

            // One SaveChanges, so statuses, fares and wallet changes commit together or not at all.
            await _uow.SaveChangesAsync();
        }

        private async Task<Driver> GetDriverAsync(Guid userId) =>
            await _uow.Drivers.GetByUserIdAsync(userId)
                ?? throw new ForbiddenException("Only drivers can manage pools.");

        private async Task<(Driver driver, Pool pool)> LoadOwnedPoolAsync(Guid userId, Guid poolId)
        {
            var driver = await GetDriverAsync(userId);
            var pool = await _uow.Pools.GetWithRideRequestsAsync(poolId)
                ?? throw new NotFoundException("Pool not found.");

            if (pool.DriverId != driver.Id)
                throw new ForbiddenException("This pool belongs to another driver.");

            return (driver, pool);
        }

        private static List<RideRequest> ActiveRides(Pool pool) =>
            pool.RideRequests.Where(r => r.Status is not ("Cancelled" or "Completed")).ToList();

        private async Task MoveAsync(RideRequest ride, string toStatus, string? reason = null)
        {
            _stateMachine.ValidateTransition(ride.Status, toStatus);

            var from = ride.Status;
            ride.Status = toStatus;
            if (toStatus is "Completed" or "Cancelled") ride.CompletedAt = DateTime.UtcNow;

            _uow.RideRequests.Update(ride);
            await RideHistoryLog.AddAsync(_uow, ride.Id, from, toStatus, reason);
        }

        // Saves after each passenger so the next pool search sees the move.
        private async Task ReassignOrCancelAsync(RideRequest ride, Driver decliningDriver)
        {
            var target = await _pooling.TryFindOrCreatePoolAsync(ride.PickupZoneId, ride.SeatsRequested, decliningDriver.Id);

            if (target == null)
            {
                await MoveAsync(ride, "Cancelled", "Driver declined and no other driver was available");
                await _uow.SaveChangesAsync();
                return;
            }

            var pickup = await _uow.Zones.GetByIdAsync(ride.PickupZoneId)
                ?? throw new NotFoundException("Pickup zone not found.");
            var dropoff = await _uow.Zones.GetByIdAsync(ride.DropoffZoneId)
                ?? throw new NotFoundException("Dropoff zone not found.");

            var occupiedSeats = target.RideRequests
                .Where(r => r.Id != ride.Id && r.Status is not ("Cancelled" or "Completed"))
                .Sum(r => r.SeatsRequested);
            var fare = _fares.CalculateFare(pickup, dropoff, ride.SeatsRequested, occupiedSeats);

            ride.PoolId = target.Id;
            ride.EstimatedFarePaisa = fare.TotalFarePaisa;
            _uow.RideRequests.Update(ride);
            await RideHistoryLog.AddAsync(_uow, ride.Id, ride.Status, ride.Status,
                "Driver declined; moved to another driver's pool and fare recalculated");
            await _uow.SaveChangesAsync();
        }

        // Only Wallet rides move money. Cash rides are collected by the driver, so nothing changes here.
        // If a Wallet ride can't be covered any more, it falls back to Cash and the ride's history says so.
        private async Task SettleFareAsync(RideRequest ride, Driver driver)
        {
            if (ride.PaymentMethod != "Wallet") return;

            var fare = ride.FinalFarePaisa ?? ride.EstimatedFarePaisa;
            var passengerWallet = await _uow.Wallets.GetByUserIdAsync(ride.PassengerId);
            var driverWallet = await _uow.Wallets.GetByUserIdAsync(driver.UserId);

            if (passengerWallet == null || driverWallet == null || passengerWallet.BalancePaisa < fare)
            {
                ride.PaymentMethod = "Cash";
                _uow.RideRequests.Update(ride);
                await RideHistoryLog.AddAsync(_uow, ride.Id, "Completed", "Completed",
                    "TeslaPay balance too low at completion; paid in cash");
                return;
            }

            passengerWallet.BalancePaisa -= fare;
            driverWallet.BalancePaisa += fare;
            _uow.Wallets.Update(passengerWallet);
            _uow.Wallets.Update(driverWallet);

            var now = DateTime.UtcNow;
            await _uow.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = passengerWallet.Id,
                RideRequestId = ride.Id,
                AmountPaisa = fare,
                Type = "Debit",
                CreatedAt = now
            });
            await _uow.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                WalletId = driverWallet.Id,
                RideRequestId = ride.Id,
                AmountPaisa = fare,
                Type = "Credit",
                CreatedAt = now
            });
        }

        private async Task<PoolDto> MapAsync(Pool pool)
        {
            var vehicle = await _uow.Vehicles.GetByIdAsync(pool.VehicleId);
            var passengers = new List<PoolPassengerDto>();

            foreach (var r in pool.RideRequests.OrderBy(r => r.RequestedAt))
            {
                var user = await _uow.Users.GetByIdAsync(r.PassengerId);
                var pickup = await _uow.Zones.GetByIdAsync(r.PickupZoneId);
                var dropoff = await _uow.Zones.GetByIdAsync(r.DropoffZoneId);

                passengers.Add(new PoolPassengerDto
                {
                    RideRequestId = r.Id,
                    PassengerName = user?.Name ?? "",
                    Seats = r.SeatsRequested,
                    PickupZoneName = pickup?.Name ?? "",
                    DropoffZoneName = dropoff?.Name ?? "",
                    Status = r.Status,
                    PaymentMethod = r.PaymentMethod,
                    FarePaisa = r.FinalFarePaisa ?? r.EstimatedFarePaisa
                });
            }

            return new PoolDto
            {
                Id = pool.Id,
                Status = pool.Status,
                VehicleName = vehicle?.Name ?? "",
                Capacity = vehicle?.Capacity ?? 0,
                SeatsTaken = ActiveRides(pool).Sum(r => r.SeatsRequested),
                CreatedAt = pool.CreatedAt,
                StartedAt = pool.StartedAt,
                CompletedAt = pool.CompletedAt,
                Passengers = passengers
            };
        }
    }
}
