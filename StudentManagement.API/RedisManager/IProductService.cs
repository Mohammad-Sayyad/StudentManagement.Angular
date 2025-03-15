using StudentManagement.API.Model;

namespace StudentManagement.API.RedisManager
{
    public interface IProductService
    {

        Task<Product?> GetProductAsync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
