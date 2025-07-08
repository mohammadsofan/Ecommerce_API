using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mshop.Api.Data.models;
using Mshop.Api.Services;
using Stripe;
using Stripe.Checkout;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IOrderService orderService;

        public StripeWebhookController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            // 1. اقرأ محتوى الطلب
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // 2. سر التوقيع من Stripe Dashboard
            const string webhookSecret = "whsec_izOQ0zpjPsGSVKgw0gpv1k6j3qm2x9SO";
            Event stripeEvent;

            try
            {
                // 3. تحقق من التوقيع - لمنع أي تلاعب
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );
            }
            catch
            {
                return BadRequest("Invalid signature");
            }

            // 4. تعامل مع الأحداث
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                var order = await orderService.GetOneAsync(o => o.SessionId == session!.Id);
                if (order is not null)
                {
                    order.OrderStatus = OrderStatus.Approved;
                    await orderService.EditAsync(order.Id, order);
                }
            }
            else if (stripeEvent.Type == EventTypes.CheckoutSessionExpired)
            {
                var session = stripeEvent.Data.Object as Session;
                var order = await orderService.GetOneAsync(o => o.SessionId == session!.Id);
                if (order is not null)
                {
                    order.OrderStatus = OrderStatus.Cancelled;
                    await orderService.EditAsync(order.Id, order);
                }
            }

            // 5. رد على Stripe
            return Ok();
        }
    }
}
