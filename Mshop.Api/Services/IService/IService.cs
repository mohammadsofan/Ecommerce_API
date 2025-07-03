using Mshop.Api.Data.Interfaces;
using Mshop.Api.Data.models;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mshop.Api.Services.IService
{
    public interface IService<T> where T : class, IEntity
    {
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? expression = null, bool isTrackable = true,params Expression<Func<T, object>>[] includes);
        Task<T?> GetOneAsync(Expression<Func<T, bool>> expression, bool isTrackable = true,params Expression<Func<T, object>>[] includes);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> EditAsync(Guid id, T entity, CancellationToken cancellationToken = default);

    }
}
