using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IPoolLifecycleService
    {
        Task<IEnumerable<PoolDto>> GetMyPoolsAsync(Guid driverUserId);
        Task<IEnumerable<PoolDto>> GetHistoryAsync(Guid driverUserId);
        Task<PoolDto> GetPoolAsync(Guid driverUserId, Guid poolId);
        Task AcceptAsync(Guid driverUserId, Guid poolId);
        Task DeclineAsync(Guid driverUserId, Guid poolId);
        Task CancelAsync(Guid driverUserId, Guid poolId);
        Task ArriveAsync(Guid driverUserId, Guid poolId);
        Task StartAsync(Guid driverUserId, Guid poolId);
        Task CompleteAsync(Guid driverUserId, Guid poolId);
    }
}
