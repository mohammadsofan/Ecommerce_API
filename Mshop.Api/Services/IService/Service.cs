using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data;
using Mshop.Api.Data.Interfaces;
using System.Linq.Expressions;

namespace Mshop.Api.Services.IService
{
    public class Service<T> : IService<T> where T : class,IEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbset;

        public Service(ApplicationDbContext context)
        {
            this._context = context;
            _dbset=context.Set<T>();
        }
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.Id = Guid.NewGuid();
            await _context.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _context.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync();
            return entities;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbset.FirstOrDefaultAsync(t=>t.Id == id);
            if (entity is null)
                return false;
            _dbset.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> EditAsync(Guid id, T entity, CancellationToken cancellationToken = default)
        {
            var entityInDB = await GetOneAsync(c => c.Id == id,false);
            if (entityInDB is null)
                return false;
            entity.Id = entityInDB.Id;
            _dbset.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? expression=null, bool isTrackable = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbset;
            if(expression is not null)
            {
                query = query.Where(expression);
            }
            if (!isTrackable)
            {
                query=query.AsNoTracking();
            }
            if(includes.Length > 0)
            {
                foreach(var include in includes)
                {
                    query=query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>> expression, bool isTrackable = true, params Expression<Func<T, object>>[] includes)
        {
            var all=await GetAsync(expression, isTrackable,includes);
            return all.FirstOrDefault();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
