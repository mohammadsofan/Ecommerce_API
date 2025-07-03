using Mshop.Api.Data.Interfaces;

namespace Mshop.Api.Data.models
{
    public enum OrderStatus
    {
        Pending,
        Approved,
        Shipped,
        Cancelled,
        Completed
    }
    public enum PaymentMethod
    {
        Cash,
        Visa
    }
    public class Order:IEntity
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippedDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? SessionId { get; set;}
        public string? TransactionId { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string RecipientPhone { get; set; } = null!;
        public decimal Subtotal { get; set; }        // مجموع أسعار المنتجات بدون توصيل
        public decimal DeliveryFee { get; set; }     // سعر التوصيل
        public decimal TotalAmount => Subtotal + DeliveryFee; // السعر الكلي
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public IEnumerable<OrderItem> OrderItems { get; set; } = null!;

    }
}
