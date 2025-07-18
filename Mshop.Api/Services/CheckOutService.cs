using Stripe.Checkout;
using Microsoft.IdentityModel.Tokens;
using Mshop.Api.Data.models;
using Stripe;
using Microsoft.AspNetCore.Http.HttpResults;
using Mshop.Api.DTOs.Requests;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Azure.Core;
namespace Mshop.Api.Services
{
    public class CheckOutService : ICheckOutService
    {
        private readonly ICartService cartService;
        private readonly IOrderService orderService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOrderItemService orderItemService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly IEmailSender emailSender;

        public CheckOutService(ICartService cartService,IOrderService orderService,UserManager<ApplicationUser> userManager
            ,IOrderItemService orderItemService,IHttpContextAccessor httpContextAccessor
            ,LinkGenerator linkGenerator,IEmailSender emailSender) {
            this.cartService = cartService;
            this.orderService = orderService;
            this.userManager = userManager;
            this.orderItemService = orderItemService;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            this.emailSender = emailSender;
        }
        public class CheckOutResult
        {
            public Session? Session { get; set; }
            public string? Message { get; set; }
            public decimal TotalAmount { get; set; }
            public Guid orderId { get; set; }
        }
        public string? GenerateActionUrl(string action, string controller, object values)
        {
            var httpContext=_httpContextAccessor.HttpContext;
            return _linkGenerator.GetUriByAction(
                    httpContext!,
                    action,
                    controller,
                    values,
                    httpContext!.Request.Scheme,
                    httpContext!.Request.Host
                );
        }
        public async Task<CheckOutResult?> CreateCheckoutSession(string userId,CheckOutRequest checkOutRequest,CancellationToken cancellationToken = default)
        {
            var cart = await cartService.GetAsync(c => c.ApplicationUserId == userId, false, includes: c => c.Product);
            if (cart.IsNullOrEmpty())
            {
                return null;
            }
            var order = new Order()
            {
                ApplicationUserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                Subtotal = cart.Sum(c=> Math.Round((c.Product.Price - c.Product.Discount*c.Product.Price) * c.Quantity)),
                DeliveryFee = checkOutRequest.DeliveryFee,
                PaymentMethod = checkOutRequest.PaymentMethod,
                ShippingAddress=checkOutRequest.ShippingAddress,
                RecipientName = checkOutRequest.RecipientName,
                RecipientPhone = checkOutRequest.RecipientPhone,
            };
            if (checkOutRequest.PaymentMethod == Data.models.PaymentMethod.Cash)
            {
                await orderService.AddAsync(order, cancellationToken);
                return (new CheckOutResult() { Message = "Order done successfully, with cash payment method.",TotalAmount=order.TotalAmount,orderId=order.Id });
            }
            else
            {
                await orderService.AddAsync(order, cancellationToken);
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = GenerateActionUrl("Success", "CheckOut",new {orderId=order.Id}),
                    CancelUrl = GenerateActionUrl("Cancel", "CheckOut", new { orderId = order.Id }),
                };
                foreach (var item in cart)
                {
                    options.LineItems.Add(
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = checkOutRequest.Currency,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Name,
                                    Description = item.Product.Description,
                                },
                                UnitAmount = (long)Math.Round((item.Product.Price - (item.Product.Price * item.Product.Discount)) * 100),
                            },
                            Quantity = item.Quantity,
                        }
                     );
                }
                options.LineItems.Add(
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = checkOutRequest.Currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "DeliveryFee",
                            },
                            UnitAmount = (long) order.DeliveryFee * 100,
                        },
                        Quantity=1
                    });
                var service = new SessionService();
                var session = service.Create(options);
                order.SessionId = session.Id;
                await orderService.EditAsync(order.Id, order);
                return (new CheckOutResult() { Session = session, Message = "Order done successfully, with visa payment method.",TotalAmount=order.TotalAmount,orderId=order.Id });
            }
        }

        public async Task<Refund> RefundPayment(string paymentId)
        {
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentId,
            };

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(refundOptions);
            return refund;
        }

        public async Task<bool> OnSuccess(Guid orderId)
        {

            var order = await orderService.GetOneAsync(o=>o.Id==orderId);
            if (order is null) return false;
            var user = await userManager.FindByIdAsync(order.ApplicationUserId);
            if (user is null) return false;
            var cart = await cartService.GetAsync(c => c.ApplicationUserId == user.Id,true,includes:c=>c.Product);
            if(cart.IsNullOrEmpty()) return false;
            List<OrderItem> orderItems= new List<OrderItem>();
            foreach (var item in cart)
            {
                var orderItem = new OrderItem()
                {
                    Id= Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    TotalAmount = Math.Round((item.Product.Price - (item.Product.Price * item.Product.Discount)) * item.Quantity),
                    Note = ""
                };
                item.Product.Quantity -= item.Quantity;
                orderItems.Add(orderItem);
            }
            await orderItemService.AddRangeAsync(orderItems);
            await cartService.ClearAsync(order.ApplicationUserId);
            var subject = "";
            var htmlMessage = "";
            if (order.PaymentMethod == Data.models.PaymentMethod.Visa)
            {
                var service = new SessionService();
                var session = service.Get(order.SessionId);
                order.OrderStatus = OrderStatus.Approved;
                order.TransactionId = session.PaymentIntentId;
                await orderService.EditAsync(orderId, order);
                subject = "Payment Successful - Order Confirmed";
                htmlMessage = $@"<div style=""font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: auto;"">
                                    <h2 style=""color: #007bff;"">Payment Successful - Order Confirmed</h2>
                                    <p>Hello,</p>
                                    <p>We have received your payment and your order is confirmed.</p>
                                    <p><strong>Payment Method:</strong> Visa / Credit Card</p>
                                    <p>Your items will be shipped shortly.</p>
                                    <p>Thank you for choosing <strong>MShop</strong>!</p>
                                    <hr style=""margin: 30px 0;"" />
                                    <p style=""font-size: 0.8em; color: #999;"">© {DateTime.UtcNow.Year} MShop. All rights reserved.</p>
                                </div>";
            }
            else if(order.PaymentMethod == Data.models.PaymentMethod.Cash)
            {
                subject = "Order Confirmed - Cash Payment";
                htmlMessage = $@"<div style=""font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: auto;"">
                                    <h2 style=""color: #28a745;"">Order Confirmed - Cash Payment</h2>
                                    <p>Hello,</p>
                                    <p>Your order has been successfully placed and will be processed shortly.</p>
                                    <p><strong>Payment Method:</strong> Cash on Delivery</p>
                                    <p>Please ensure someone is available to receive and pay for the order at the delivery address.</p>
                                    <p>Thank you for shopping with <strong>MShop</strong>!</p>
                                    <hr style=""margin: 30px 0;"" />
                                    <p style=""font-size: 0.8em; color: #999;"">© {DateTime.UtcNow.Year} MShop. All rights reserved.</p>
                                </div>
";
            }
            await emailSender.SendEmailAsync(user.Email!, subject, htmlMessage);
            return true;

        }

        public async Task<bool> OnCancel(Guid orderId)
        {
            var order = await orderService.GetOneAsync(o => o.Id == orderId);
            if (order is null) return false;
            var user = await userManager.FindByIdAsync(order.ApplicationUserId);
            if(user is null) return false;

            order.OrderStatus = OrderStatus.Cancelled;
            await orderService.EditAsync(orderId,order);
            var subject = "Order Cancelled - Payment Not Completed";
            var htmlMessage = $@"
                <div style=""font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: auto;"">
                    <h2 style=""color: #dc3545;"">Order Cancelled</h2>
                    <p>Hello,</p>
                    <p>We noticed that your order could not be completed because the payment was not finalized.</p>
                    <p><strong>Reason:</strong> Payment session expired or was not completed.</p>
                    <p>If this was a mistake, you can place your order again at any time.</p>
                    <p>Need help? Contact our support team and we'll be happy to assist you.</p>
                    <p>Thank you for choosing <strong>MShop</strong>.</p>
                    <hr style=""margin: 30px 0;"" />
                    <p style=""font-size: 0.8em; color: #999;"">© {DateTime.UtcNow.Year} MShop. All rights reserved.</p>
                </div>";
            await emailSender.SendEmailAsync(user.Email!, subject, htmlMessage);
            return true;
        }
    }
}
