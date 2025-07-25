using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Requests;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mshop.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext context;
        private readonly ICategoryService categoryService;
        private readonly IBrandService brandService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ProductService(ApplicationDbContext context,ICategoryService categoryService,IBrandService brandService,IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.categoryService = categoryService;
            this.brandService = brandService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<Product> AddAsync(Product product, IFormFile file,CancellationToken cancellationToken = default)
        {
            var category = await categoryService.GetOneAsync(c => c.Id == product.CategoryId, false);
            if(category == null)
            {
                throw new InvalidOperationException($"Category with id {product.CategoryId} does not exist.");
            }
            if(product.BrandId.HasValue)
            {
                var brand = await brandService.GetOneAsync(b => b.Id == product.BrandId, false);
                if(brand == null)
                {
                    throw new InvalidOperationException($"Brand with id {product.BrandId} does not exist.");
                }
            }
            if (file is null || file.Length <= 0)
            {
                throw new InvalidDataException("Invalid File");
            }
            var extension = Path.GetExtension(file.FileName);
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidDataException("Invalid File Format");

            }
            var fileName = Guid.NewGuid() + file.FileName;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", fileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            product.MainImage = httpContextAccessor.HttpContext!.Request.Scheme + "://" + httpContextAccessor.HttpContext.Request.Host + "/images/" + fileName;
            product.Id = Guid.NewGuid();
            await context.Products.AddAsync(product, cancellationToken);
            await context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = context.Products.Find(id);
            if (product is null)
            {
                return false;
            }
            var splits = product.MainImage.Split("/");
            var fileName = splits[splits.Length - 1];
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Images",fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            context.Products.Remove(product);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> EditAsync(Guid id, Product product,IFormFile? file, CancellationToken cancellationToken = default)
        {
            var productInDb = context.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (productInDb is null)
            {
                return false;
            }
            var category = await categoryService.GetOneAsync(c => c.Id == product.CategoryId, false);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with id {product.CategoryId} does not exist.");
            }
            if (product.BrandId.HasValue)
            {
                var brand = await brandService.GetOneAsync(b => b.Id == product.BrandId, false);
                if (brand == null)
                {
                    throw new InvalidOperationException($"Brand with id {product.BrandId} does not exist.");
                }
            }
            if (file is not null && file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidDataException("Invalid File Format");
                }
                var splits = productInDb.MainImage.Split("/");
                var currentFileName = splits[splits.Length - 1];
                var currentPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", currentFileName);
                if (System.IO.File.Exists(currentPath))
                {
                    System.IO.File.Delete(currentPath);
                }
                var fileName = Guid.NewGuid() + file.FileName;
                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", fileName);
                using (var stream = System.IO.File.Create(newPath))
                {
                   await file.CopyToAsync(stream);
                }
                product.MainImage = httpContextAccessor.HttpContext!.Request.Scheme + "://" + httpContextAccessor.HttpContext.Request.Host + "/images/" + fileName;
            }
            else
            {
                product.MainImage = productInDb.MainImage;
            }
            product.Id = productInDb.Id;
            context.Products.Update(product);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Product?> GetOneAsync(Expression<Func<Product, bool>> expression,bool isTrackable = true ,params Expression<Func<Product, object>>[] includes)
        {
            IQueryable<Product> query = context.Products;
            if (!isTrackable)
            {
                query = query.AsNoTracking();
            }
            if (includes.Length > 0) {
                foreach (var include in includes) { 
                    query = query.Include(include);
                }
            }
            return await query.FirstOrDefaultAsync(expression);
        }

        public async Task<IEnumerable<Product>> GetAsync(string? query, int page = 1, int limit=10, bool isTrackable = true, params Expression<Func<Product, object>>[] includes)
        {
            if (limit <= 0) limit = 10;
            if (page <= 0) page = 1;
            IQueryable<Product> products = context.Products;
            if (!isTrackable)
            {
                products = products.AsNoTracking();
            }
            if (includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    products = products.Include(include);
                }
            }
            if (!string.IsNullOrEmpty(query))
            {
                products = products.Where(p => (p.Name.Contains(query) || p.Description.Contains(query)));
            }

            return await products.Skip((page-1) * limit).Take(limit).ToListAsync();
        }
        public async Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await context.Products.FindAsync(id);
            if (product is null)
            {
                return false;
            }
            product.Status = !product.Status;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> IncreaseQuantity(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await context.Products.FindAsync(id);
            if (product is null)
            {
                return false;
            }
            product.Quantity +=1;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DecreaseQuantity(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await context.Products.FindAsync(id);
            if (product is null)
            {
                return false;
            }
            product.Quantity -= 1;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
