using BLL.Interfaces;

namespace AppLayer.Workers
{
    // Runs inside the API process (no queue or extra service needed at MVP scale).
    public class StaleRideExpiryWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly IConfiguration _config;
        private readonly ILogger<StaleRideExpiryWorker> _logger;

        public StaleRideExpiryWorker(
            IServiceScopeFactory scopes, IConfiguration config, ILogger<StaleRideExpiryWorker> logger)
        {
            _scopes = scopes;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var maxWait = TimeSpan.FromMinutes(_config.GetValue<int>("RideExpiry:MaxWaitMinutes", 10));
            var interval = TimeSpan.FromSeconds(_config.GetValue<int>("RideExpiry:CheckIntervalSeconds", 60));

            using var timer = new PeriodicTimer(interval);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        using var scope = _scopes.CreateScope();
                        var expiry = scope.ServiceProvider.GetRequiredService<IRideExpiryService>();
                        var expired = await expiry.ExpireStaleAsync(maxWait);
                        if (expired > 0)
                            _logger.LogInformation("Expired {Count} ride request(s) with no driver.", expired);
                    }
                    catch (Exception ex)
                    {
                        // A failed tick (DB down, lock timeout) must not kill the worker.
                        _logger.LogError(ex, "Ride expiry tick failed.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
        }
    }
}
