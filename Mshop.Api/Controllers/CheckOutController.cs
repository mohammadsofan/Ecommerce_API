using Microsoft.AspNetCore.Mvc;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Mshop.Api.Utilities;
using Stripe;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckOutController : ControllerBase
    {
        private readonly ICheckOutService _checkOutService;

        public CheckOutController(ICheckOutService checkOutService)
        {
            this._checkOutService = checkOutService;
        }

        [HttpPost("CreateCheckoutSession")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckOutRequest checkOutRequest,CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                if (userId is null)
                {
                    return Unauthorized();
                }
                var checkOutResult = await _checkOutService.CreateCheckoutSession(userId, checkOutRequest, cancellationToken);

                if(checkOutResult is null) {
                    return BadRequest(new { message = "User's cart is empty" });
                }
                if (checkOutRequest.PaymentMethod == Data.models.PaymentMethod.Cash)
                {
                    return RedirectToAction(nameof(Success), new { OrderId = checkOutResult.orderId });
                }
                else
                {
                    return Ok(new { checkOutResult.Message, PaymentMethod = "Visa", checkOutResult.TotalAmount,OrderId = checkOutResult.orderId, SessionId=checkOutResult.Session!.Id, checkOutResult.Session.Url });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        new { message = "Payment failed", error = ex.Message });
            }
        }
        [HttpGet("success/{orderId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Success([FromRoute] Guid orderId)
        {
            try
            {
                await _checkOutService.OnSuccess(orderId);
                return Ok(new { message = "payment done successfully!"});
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("cancel/{orderId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel([FromRoute] Guid orderId)
        {
            try
            {
                await _checkOutService.OnCancel(orderId);
                return Ok(new { message = "payment proccess cancelled!" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("refund")]
        [Authorize(Roles =$"{ApplicationRoles.SuperAdmin}")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundRequest request)
        {
            try
            {

                var refund = await _checkOutService.RefundPayment(request.PaymentId);

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
