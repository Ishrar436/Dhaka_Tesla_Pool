using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IPoolingService
    {
        Task<Pool> FindOrCreatePoolAsync(Guid pickupZoneId, Guid dropoffZoneId, int seatsRequested);
    }
}
