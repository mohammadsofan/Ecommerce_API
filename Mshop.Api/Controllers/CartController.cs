using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.DTOs.Responses;
using Mshop.Api.Services;
using System.Security.Claims;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService cartService;
        private readonly UserManager<ApplicationUser> userManager;

        public CartController(ICartService cartService,UserManager<ApplicationUser> userManager) {
            this.cartService = cartService;
            this.userManager = userManager;
        }
        public Guid? GetCurrentUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(id, out var userId) ? userId : null;
        }
        [HttpGet("")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                if (GetCurrentUserId() is not Guid userId)
                    return Unauthorized();
                var cart = await cartService.GetAsync(c => c.ApplicationUserId == userId, false,c=>c.Product);
                var totalAmount = cart.Sum(c => (c.Product.Price - (c.Product.Price * c.Product.Discount)) * c.Quantity);
                return Ok(new { CartItems=cart.Adapt<IEnumerable<CartResponse>>(),TotalPrice=totalAmount });
                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOneCartItem([FromRoute] Guid id)
        {
            try
            {
                if (GetCurrentUserId() is not Guid userId)
                    return Unauthorized();

                var cart = await cartService.GetOneAsync(c => c.ApplicationUserId == userId&&c.ProductId==id, false, c => c.Product);
                return Ok(cart.Adapt<CartResponse>());

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart([FromRoute] Guid id,CancellationToken cancellationToken = default)
        {
            try
            {
                if (GetCurrentUserId() is not Guid userId)
                    return Unauthorized();
                var result = await cartService.DeleteAsync(userId,id, cancellationToken);
                if(!result) return NotFound();
                return NoContent();

            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("")]
        public async Task<IActionResult> AddToCart([FromBody] CartRequest cartRequest,CancellationToken cancellationToken = default)
        {
            try
            {
                if (GetCurrentUserId() is not Guid userId)
                    return Unauthorized();

                var exisitingCart = await cartService.GetOneAsync(c=>c.ProductId==cartRequest.ProductId && c.ApplicationUserId == userId);
                if (exisitingCart is not null) {
                    var newQuantity = await cartService.EditQuantityAsync(userId, cartRequest.ProductId, exisitingCart.Quantity + 1, cancellationToken);
                    if(newQuantity == -99)
                    {
                        return NotFound();
                    }
                    return Ok(new { Message = $"Product quantity increased by 1, current quantity is {newQuantity}" });
                }
                var cart = cartRequest.Adapt<Cart>();
                cart.ApplicationUserId = userId;
                cart = await cartService.AddAsync(cart, cancellationToken);
                return CreatedAtAction(nameof(GetCartItems),null);


            }

            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateQuantity([FromRoute] Guid id,[FromBody] UpdateQuantityRequest quantityRequest,CancellationToken cancellationToken = default)
        {
            try
            {

                if (GetCurrentUserId() is not Guid userId)
                    return Unauthorized();

                var result = await cartService.EditQuantityAsync(userId, id, quantityRequest.Quantity, cancellationToken);
                if (result==-99) return NotFound();
                return NoContent();

            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("ClearCartItems")]
        public async Task<IActionResult> Clear()
        {
            try { 
                if (GetCurrentUserId() is not Guid userId)
                    return Unauthorized();
                await cartService.Clear(userId);
                return NoContent();
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
