using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services
{
    public class ResetPasswordCodeService : Service<ResetPasswordCode>, IResetPasswordCodeService
    {
        private readonly ApplicationDbContext context;
        public ResetPasswordCodeService(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
