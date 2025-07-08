using Mshop.Api.Data.models;
using System.Linq.Expressions;

namespace Mshop.Api.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAsync(string? query, int page = 1, int limit = 10, bool isTrackable = true, params Expression<Func<Product, object>>[] includes);
        Task<Product?> GetOneAsync(Expression<Func<Product, bool>> expression, bool isTrackable = true, params Expression<Func<Product, object>>[] includes);
        Task<Product> AddAsync(Product product,IFormFile image,CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> EditAsync(Guid id, Product product, IFormFile? image, CancellationToken cancellationToken = default);
        Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> IncreaseQuantity(Guid id, CancellationToken cancellationToken = default);
        Task<bool> DecreaseQuantity(Guid id, CancellationToken cancellationToken = default);

    }
}
