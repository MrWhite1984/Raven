
namespace Raven.BackgroundServices
{
    public class LogsDropper : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Logger.Logger.FlushBuffer();
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
