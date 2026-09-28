using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IPoolLifecycleService
    {
        Task<IEnumerable<PoolDto>> GetMyPoolsAsync(Guid driverUserId);
        Task<PoolDto> GetPoolAsync(Guid driverUserId, Guid poolId);
        Task AcceptAsync(Guid driverUserId, Guid poolId);
        Task StartAsync(Guid driverUserId, Guid poolId);
        Task CompleteAsync(Guid driverUserId, Guid poolId);
    }
}
