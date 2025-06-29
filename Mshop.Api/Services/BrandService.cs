using Mshop.Api.Data.models;
using Mshop.Api.Data;
using Mshop.Api.Services.IService;
using Mshop.Api.Services.IStatusService;

namespace Mshop.Api.Services
{
    public class BrandService: StatusService<Brand>,IBrandService
    {
        private readonly ApplicationDbContext context;

        public BrandService(ApplicationDbContext context):base(context)
        {
            this.context = context;
        }

    }
}
