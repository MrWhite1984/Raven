using Microsoft.Extensions.Logging;
using Raven.DB.PSQL;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.Logger;
using Raven.DB.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
                DateTime = DateTime.UtcNow
            };
            var db = new RedisDbContext();
            db.WriteAsync(JsonSerializer.Serialize(log));
        }

        public static void FlushBuffer()
        {
            var logs = new RedisDbContext()
                .ReadAsync()
                .Result
                .Select(o=>JsonSerializer.Deserialize<Logs>(o))
                .ToList();
            LogsHandler.ImportLogsAsync(logs);
            new RedisDbContext().ClearAsync();
        }

    }
}
