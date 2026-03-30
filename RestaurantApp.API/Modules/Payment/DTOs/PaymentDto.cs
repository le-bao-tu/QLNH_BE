namespace RestaurantApp.API.Modules.Payment.DTOs
{
    public class CreatePaymentDto
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "cash";
        public string? ReferenceCode { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int TableNumber { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? ReferenceCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
