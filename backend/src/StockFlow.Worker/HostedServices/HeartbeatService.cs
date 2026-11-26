namespace StockFlow.Worker.HostedServices
{
    /// <summary>
    /// Service nền tạm thời: ghi log mỗi phút để biết Worker còn sống.
    /// Các consumer thật (RabbitMQ, Kafka, Service Bus, Import Excel) sẽ được thêm ở các bước F3–F6.
    /// </summary>
    public class HeartbeatService : BackgroundService
    {
        private readonly ILogger<HeartbeatService> _logger;
        public HeartbeatService(ILogger<HeartbeatService> logger)
        {
            _logger= logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StockFlow Worker started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker heartbeat at {Time}", DateTime.UtcNow);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
