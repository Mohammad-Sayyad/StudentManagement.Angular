using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

using StackExchange.Redis;
using StudentManagement.API.Model;
using StudentManagement.API.Repository;

namespace StudentManagement.API.RedisManager
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repository;
        private readonly IDistributedCache _cache;
        private readonly ISubscriber _subscriber;

        public ProductService(IProductRepository repository , IDistributedCache cache,
            IConnectionMultiplexer subscriber)
        {
            this.repository = repository;
            this._cache = cache;
            _subscriber = subscriber.GetSubscriber();
        }
        public async Task AddProductAsync(Product product)
        {
            await repository.AddAsync(product);
            await _cache.SetStringAsync($"product_{product.Id}", JsonSerializer.Serialize(product));
            await _subscriber.PublishAsync("cache_invalidation", product.Description.ToString());
        }
   
       
        public async Task DeleteProductAsync(int id)
        {
            await repository.DeleteAsync(id);
            await _cache.RemoveAsync($"product_{id}");
            await _subscriber.PublishAsync("cache_invalidation", id.ToString());
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await repository.GetAllAsync();
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            string cacheKey = $"product_{id}";
            var cachedProduct = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProduct))
            {
                return JsonSerializer.Deserialize<Product>(cachedProduct);
            }

            var product = await repository.GetByIdAsync(id);
            if(product != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
                {

                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
            }

            return product;
        }
        
        public async Task UpdateProductAsync(Product product)
        {
            await repository.UpdateAsync(product);
            await _cache.SetStringAsync($"product_{product.Id}", JsonSerializer.Serialize(product));
            await _subscriber.PublishAsync("cache_invalidation", product.Id.ToString());
        }
    }
}
