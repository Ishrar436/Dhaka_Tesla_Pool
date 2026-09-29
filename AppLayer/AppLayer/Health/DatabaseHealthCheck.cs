using DAL.EF;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppLayer.Health
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly TeslaDbContext _db;

        public DatabaseHealthCheck(TeslaDbContext db) => _db = db;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _db.Database.CanConnectAsync(cancellationToken)
                    ? HealthCheckResult.Healthy("Database reachable.")
                    : HealthCheckResult.Unhealthy("Cannot connect to the database.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database check failed.", ex);
            }
        }
    }
}
