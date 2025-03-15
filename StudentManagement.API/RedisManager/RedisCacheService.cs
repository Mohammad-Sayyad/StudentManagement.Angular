using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace StudentManagement.API.RedisManager
{
    public class RedisCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ISubscriber _subscriber;

        public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _subscriber = redis.GetSubscriber();

            _subscriber.Subscribe("cache_invalidation", async (channel, message) =>
            {
                try
                {
                    string productId = message.ToString();
                    await _cache.RemoveAsync($"product_{productId}");
                    Console.WriteLine($"Cache cleared for Product ID: {productId}");
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error clearing cache for Product ID: {message}. Error: {ex.Message}");
                }
            });
        }
    }
}
