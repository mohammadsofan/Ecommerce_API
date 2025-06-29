using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services
{
    public class CartService:Service<Cart>, ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context) :base(context) 
        {
            this._context = context;
        }

        public async Task<bool> CheckExists(Guid productId,Guid userId)
        {
            return await _context.Carts.AnyAsync(c=>c.ProductId == productId&&c.ApplicationUserId==userId); 
        }

        public async Task<bool> Clear(Guid userId, CancellationToken cancellationToken = default)
        {
            var cartItems = await _context.Carts.Where(c => c.ApplicationUserId == userId).ToListAsync(cancellationToken);
            if(!cartItems.Any())
                 return false;
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid userId,Guid productId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Carts.FindAsync(productId, userId);
            if (entity is null)
                return false;
            _context.Carts.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<int> EditQuantityAsync(Guid userId, Guid productId,int newQuantity, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Carts.FindAsync(productId, userId);
            if (entity is null)
                return -99;
            entity.Quantity = newQuantity;
            await _context.SaveChangesAsync(cancellationToken);
            return newQuantity;
        }
    }
}
