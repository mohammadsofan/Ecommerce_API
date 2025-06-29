using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data;
using Mshop.Api.Data.Interfaces;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services.IStatusService
{
    public class StatusService<T>: Service<T>, IStatusService<T> where T : class, IEntityStatus
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbset;

        public StatusService(ApplicationDbContext context):base(context)
        {
            this._context = context;
            _dbset = context.Set<T>();
        }
        public async Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
        {

            var entity = await _dbset.FindAsync(id);
            if (entity is null)
            {
                return false;
            }
            entity.Status = !entity.Status;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
