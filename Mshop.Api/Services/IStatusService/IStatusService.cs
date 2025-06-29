
using Mshop.Api.Data.Interfaces;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services.IStatusService
{
    public interface IStatusService<T>:IService<T> where T : class,IEntityStatus
    {
        Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
