using Mshop.Api.DTOs.Requests;
using Stripe;
using Stripe.Checkout;
using static Mshop.Api.Services.CheckOutService;

namespace Mshop.Api.Services
{
    public interface ICheckOutService
    {
        Task<CheckOutResult?> CreateCheckoutSession(string userId, CheckOutRequest checkOutRequest, CancellationToken cancellationToken = default);
        Task<bool> OnSuccess(Guid orderId);
        Task<bool> OnCancel (Guid orderId);
        Task<Refund> RefundPayment(string paymentId);
    }
}
