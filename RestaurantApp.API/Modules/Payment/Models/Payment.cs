using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Payment.Models
{
    /// <summary>Thanh toán - hỗ trợ nhiều lần thanh toán cho 1 đơn (tách bill)</summary>
    public class Payment : BaseEntity
    {
        /// <summary>Đơn hàng được thanh toán</summary>
        public Guid OrderId { get; set; }
        public Order.Models.Order? Order { get; set; }

        /// <summary>Số tiền thanh toán lần này</summary>
        public decimal Amount { get; set; }

        /// <summary>Phương thức: cash/credit_card/debit_card/momo/zalopay/vnpay/bank_transfer</summary>
        public string Method { get; set; } = "cash";

        /// <summary>Mã giao dịch từ cổng thanh toán bên ngoài</summary>
        public string? ReferenceCode { get; set; }

        /// <summary>Trạng thái: pending/completed/failed/refunded</summary>
        public string Status { get; set; } = "pending";

        /// <summary>Thời điểm thanh toán thành công</summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>Nhân viên xử lý thanh toán (employee_id)</summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>Ghi chú thanh toán</summary>
        public string? Note { get; set; }
    }

    public static class PaymentMethods
    {
        public const string Cash = "cash";
        public const string CreditCard = "credit_card";
        public const string DebitCard = "debit_card";
        public const string Momo = "momo";
        public const string ZaloPay = "zalopay";
        public const string VnPay = "vnpay";
        public const string BankTransfer = "bank_transfer";
    }

    public static class PaymentStatuses
    {
        public const string Pending = "pending";
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Refunded = "refunded";
    }
}
