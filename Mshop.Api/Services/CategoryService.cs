using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.Services.IStatusService;

namespace Mshop.Api.Services
{
    public class CategoryService : StatusService<Category>,ICategoryService
    {
        private readonly ApplicationDbContext context;

        public CategoryService(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

    }
}
