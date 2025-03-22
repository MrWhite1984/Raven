using StackExchange.Redis;

namespace Raven.DB.Redis
{
    public class RedisCacheDbContext
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase db;

        public RedisCacheDbContext()
        {
            connectionMultiplexer = ConnectionMultiplexer.Connect("redis-cache:6379,abortConnect=false");
            db = connectionMultiplexer.GetDatabase();
        }

        public async Task WriteAsync(string key, string cache)
        {
            await db.StringSetAsync(key, cache, TimeSpan.FromMinutes(10));
            Dispose();
        }

        public string? ReadCacheAsync(string key)
        {
            var result = db.StringGet(key);
            db.KeyDelete(key);
            Dispose();
            return result;
        }

        public void Dispose()
        {
            connectionMultiplexer?.Dispose();
        }
    }
}
