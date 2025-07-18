using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services
{
    public class ReviewService:Service<Review>,IReviewService
    {
        private readonly ApplicationDbContext context;
        public ReviewService(ApplicationDbContext context):base(context) 
        {
            this.context = context;
        }

    }
}
