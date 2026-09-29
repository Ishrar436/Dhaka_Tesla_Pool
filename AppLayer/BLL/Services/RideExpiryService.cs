using BLL.Helpers;
using BLL.Interfaces;
using DAL.UnitOfWork;

namespace BLL.Services
{
    public class RideExpiryService : IRideExpiryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRideStateMachineService _stateMachine;

        public RideExpiryService(IUnitOfWork uow, IRideStateMachineService stateMachine)
        {
            _uow = uow;
            _stateMachine = stateMachine;
        }

        public async Task<int> ExpireStaleAsync(TimeSpan maxWait)
        {
            var cutoff = DateTime.UtcNow - maxWait;

            // Cheap look first so an idle system doesn't take the matching lock every tick.
            if (!(await _uow.RideRequests.GetStaleWaitingAsync(cutoff)).Any())
                return 0;

            await using var tx = await _uow.BeginTransactionAsync();
            await _uow.AcquireMatchingLockAsync();

            // Re-read under the lock: a driver may have accepted in the meantime.
            var stale = (await _uow.RideRequests.GetStaleWaitingAsync(cutoff)).ToList();
            if (stale.Count == 0)
            {
                await tx.CommitAsync();
                return 0;
            }

            var reason = $"Expired: no driver accepted within {(int)maxWait.TotalMinutes} minutes";
            foreach (var ride in stale)
            {
                _stateMachine.ValidateTransition(ride.Status, "Cancelled");
                var from = ride.Status;
                ride.Status = "Cancelled";
                ride.CompletedAt = DateTime.UtcNow;
                await RideHistoryLog.AddAsync(_uow, ride.Id, from, "Cancelled", reason);
            }
            await _uow.SaveChangesAsync();

            // Free any driver whose pool is now empty.
            foreach (var poolId in stale.Where(r => r.PoolId != null).Select(r => r.PoolId!.Value).Distinct())
            {
                var pool = await _uow.Pools.GetWithRideRequestsAsync(poolId);
                if (pool != null && pool.Status == "Waiting" &&
                    pool.RideRequests.All(r => r.Status is "Cancelled" or "Completed"))
                {
                    pool.Status = "Cancelled";
                }
            }

            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return stale.Count;
        }
    }
}
