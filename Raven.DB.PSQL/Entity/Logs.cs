using Microsoft.Extensions.Logging;

namespace Raven.DB.PSQL.Entity
{
    public class Logs
    {
        public Guid Id { get; set; }
        public string LogSender { get; set; }
        public LogLevel LogLevel { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
    }
}
