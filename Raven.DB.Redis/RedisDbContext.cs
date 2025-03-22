using StackExchange.Redis;

namespace Raven.DB.Redis
{
    public class RedisDbContext
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase db;
        const string LogsKey = "logs";

        public RedisDbContext()
        {
            connectionMultiplexer = ConnectionMultiplexer.Connect("redis-logs:6379,abortConnect=false");
            db = connectionMultiplexer.GetDatabase();
        }

        public async Task WriteAsync(string log)
        {
            await db.ListRightPushAsync(LogsKey, log);
            Dispose();
        }

        public async Task<List<string>> ReadAsync()
        {
            var data = await db.ListRangeAsync(LogsKey);
            Dispose();
            return data.ToStringArray().ToList();
        }

        public async Task ClearAsync()
        {
            await db.KeyDeleteAsync(LogsKey);
            Dispose();
        }

        public void Dispose()
        {
            connectionMultiplexer?.Dispose();
        }
    }
}
