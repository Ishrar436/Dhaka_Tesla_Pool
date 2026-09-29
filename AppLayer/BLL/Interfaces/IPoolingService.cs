using DAL.EF.Tables;

namespace BLL.Interfaces
{
    public interface IPoolingService
    {
        // Throws InvalidOperationException when nothing is available.
        Task<Pool> FindOrCreatePoolAsync(Guid pickupZoneId, int seatsRequested, Guid? excludeDriverId = null);

        // Returns null when nothing is available. excludeDriverId is used when a driver declines.
        Task<Pool?> TryFindOrCreatePoolAsync(Guid pickupZoneId, int seatsRequested, Guid? excludeDriverId = null);
    }
}
