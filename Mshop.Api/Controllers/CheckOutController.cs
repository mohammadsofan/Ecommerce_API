using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using Mshop.Api.DTOs.Requests;
using System.Net.Sockets;
using Mshop.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Mshop.Api.Utilities;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckOutController : ControllerBase
    {
        private readonly ICartService cartService;

        public CheckOutController(ICartService cartService)
        {
            this.cartService = cartService;
        }

        [HttpPost("CreateCheckoutSession")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckOutRequest model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = Guid.TryParse(userId, out var id);
            if(!result)
            {
                return Unauthorized();
            }
            var cart = await cartService.GetAsync(c => c.ApplicationUserId == id, false, includes: c => c.Product);
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = Url.Action(nameof(Success),"CheckOut",null,protocol:Request.Scheme,host:Request.Host.Value),
                CancelUrl = Url.Action(nameof(Cancel), "CheckOut", null, protocol: Request.Scheme, host: Request.Host.Value),
            };
            foreach (var item in cart)
            {
                options.LineItems.Add(
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = model.Currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name =item.Product.Name,
                                Description = item.Product.Description,
                            },
                            UnitAmount =(long) (item.Product.Price - (item.Product.Price*item.Product.Discount))*100,
                        },
                        Quantity = item.Quantity,
                    }
                 );
            }
            var service = new SessionService();
            var session = service.Create(options);
            return Ok(new { sessionId = session.Id, url = session.Url });
        }
        [HttpGet("success")]
        [AllowAnonymous]
        public IActionResult Success()
        {
            return Ok(new {message="payment done successfully!"});
        }
        [HttpGet("cancel")]
        [AllowAnonymous]
        public IActionResult Cancel()
        {
            return Ok(new {message="payment proccess canceled!"});
        }
        [HttpPost("refund")]
        [Authorize(Roles =$"{ApplicationRoles.SuperAdmin}")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundRequest request)
        {
            try
            {
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = request.PaymentId,
                };

                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(refundOptions);

                if (refund.Status == "succeeded")
                {
                    return Ok(new
                    {
                        message = "Refund is successfully done",
                        refundId = refund.Id,
                        amount = refund.Amount,
                        currency = refund.Currency
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Refund failed",
                        status = refund.Status
                    });
                }
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = "Stripe error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Refund failed", error = ex.Message });
            }
        }

    }
}
