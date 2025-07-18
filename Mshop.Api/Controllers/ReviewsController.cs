using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.DTOs.Responses;
using Mshop.Api.Services;
using Mshop.Api.Utilities;
using System.Security.Claims;
using System.Threading;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService reviewService;
        private readonly IOrderService orderService;
        private readonly IOrderItemService orderItemService;
        private readonly IProductService productService;
        private readonly UserManager<ApplicationUser> userManager;

        public ReviewsController(IReviewService reviewService,IOrderService orderService,IOrderItemService orderItemService
            , IProductService productService, UserManager<ApplicationUser> userManager){
            this.productService = productService;
            this.userManager = userManager;
            this.reviewService = reviewService;
            this.orderService = orderService;
            this.orderItemService = orderItemService;
        }
        [HttpGet("Product/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductReviews([FromRoute] Guid id)
        {
            try
            {
                var product = await productService.GetOneAsync(p => p.Id == id);
                if(product == null)
                {
                    return NotFound(new { Message = "Product not found" });
                }
                var reviews = await reviewService.GetAsync(r=>r.ProductId == id,false,r=>r.ApplicationUser,r=>r.Product);
                return Ok(new { ProductId = id,reviews = reviews.Adapt<IEnumerable<ReviewResponse>>()});
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
            
        }
        [HttpGet("User/{id}")]
        [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
        public async Task<IActionResult> GetUserReviews([FromRoute] string id)
        {
            try
            {
                var user = userManager.Users.FirstOrDefault(u => u.Id == id);
                if(user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }
                var reviews = await reviewService.GetAsync(r => r.ApplicationUserId == id, false, r => r.ApplicationUser, r => r.Product);
                return Ok(new { UserId = id, reviews = reviews.Adapt<IEnumerable<ReviewResponse>>() });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id,CancellationToken cancellationToken = default)
        {
            try
            {
                var review = await reviewService.GetOneAsync(r => r.Id == id);
                if (review is null)
                    return NotFound();
                await reviewService.DeleteAsync(id, cancellationToken);
                var product = await productService.GetOneAsync(p => p.Id == review.ProductId);
                if (product is not null)
                {
                    var reviews = await reviewService.GetAsync(r => r.ProductId == review.ProductId, false);
                    product.ReviewsCount = reviews.Count();
                    product.Rate = reviews.Any()
                        ? reviews.Average(r => r.Rate)
                        : 0;
                    await productService.SaveChangesAsync();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

        [HttpPost("product/{id}")]
        public async Task<IActionResult> Create([FromRoute] Guid id,[FromBody] ReviewRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await productService.GetOneAsync(p => p.Id == id);
                if (product == null)
                {
                    return NotFound(new { Message = "Product not found" });
                }
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var orders = await orderService.GetAsync(o => o.ApplicationUserId == userId && o.OrderStatus == OrderStatus.Completed
                , false,o=>o.OrderItems);
                if (!orders.Any())
                {
                    return BadRequest(new { Message = "You can only review products you’ve purchased and completed." });
                }
                var orderedProductIds = orders.SelectMany(o=>o.OrderItems).Select(oi=>oi.ProductId).ToHashSet();
                if (!orderedProductIds.Contains(id))
                {
                    return BadRequest(new { Message = "You can only review products you’ve purchased and completed." });
                }
                var existingReview = await reviewService.GetOneAsync(r => r.ProductId == id && r.ApplicationUserId == userId);
                if (existingReview != null)
                {
                    return BadRequest(new { Message = "You have already reviewed this product." });
                }

                var review = request.Adapt<Review>();
                review.ApplicationUserId = userId;
                review.ProductId = id;
                await reviewService.AddAsync(review, cancellationToken);
                var reviews = await reviewService.GetAsync(r => r.ProductId == product.Id, false);
                product.ReviewsCount = reviews.Count();
                product.Rate = reviews.Average(o=>o.Rate);
                await productService.SaveChangesAsync();
                return Created();
                
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { Message = "You have already reviewed this product." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, ReviewRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var review = await reviewService.GetOneAsync(r => r.Id == id, false);
                if(review == null)
                {
                    return NotFound();
                }
                if(review.ApplicationUserId != userId)
                {
                    return Forbid();
                }

                var updatedReview = request.Adapt<Review>();
                updatedReview.ApplicationUserId = userId;
                var result = await reviewService.EditAsync(id, updatedReview, cancellationToken);
                if (!result)
                {
                    return NotFound();
                }
                if (request.Rate != review.Rate)
                {
                    var product = await productService.GetOneAsync(p => p.Id == review.ProductId);
                    if (product is not null)
                    {
                        var reviews = await reviewService.GetAsync(r => r.ProductId == review.ProductId, false);
                        product.ReviewsCount = reviews.Count();
                        product.Rate = reviews.Any()
                            ? reviews.Average(r => r.Rate)
                            : 0;
                        await productService.SaveChangesAsync();
                    }
                }
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

    }
}
