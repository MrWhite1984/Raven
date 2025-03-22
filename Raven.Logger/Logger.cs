using Microsoft.Extensions.Logging;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.Logger;
using Raven.DB.Redis;
using System.Text.Json;

namespace Raven.Logger
{
    public class Logger
    {

        public static void Log(LogLevel logLevel, string message)
        {
            Logs log = new Logs()
            {
                LogLevel = logLevel,
                Message = message,
                DateTime = DateTime.UtcNow,
                LogSender = "Raven"
            };
            var db = new RedisLogsDbContext();
            db.WriteAsync(JsonSerializer.Serialize(log));
        }

        public static void FlushBuffer()
        {
            var dbLogs = new RedisLogsDbContext()
                .ReadAsync()
                .Result
                .ToList();
            var logs = dbLogs.Select(o => JsonSerializer.Deserialize<Logs>(o)).ToList();
            LogsHandler.ImportLogsAsync(logs);
            new RedisLogsDbContext().ClearAsync();
        }

    }
}
