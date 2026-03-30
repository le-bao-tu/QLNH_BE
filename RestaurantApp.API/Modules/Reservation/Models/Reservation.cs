using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Reservation.Models
{
    /// <summary>Đặt bàn trước từ khách hàng</summary>
    public class Reservation : BaseEntity
    {
        /// <summary>Chi nhánh đặt bàn</summary>
        public Guid BranchId { get; set; }
        public Branch.Models.Branch? Branch { get; set; }

        /// <summary>Bàn được đặt (nullable nếu chưa gán cụ thể)</summary>
        public Guid? TableId { get; set; }
        public Table.Models.Table? Table { get; set; }

        /// <summary>Khách hàng đặt (nullable nếu chưa có tài khoản)</summary>
        public Guid? CustomerId { get; set; }
        public Customer.Models.Customer? Customer { get; set; }

        /// <summary>Tên khách (nếu chưa có tài khoản)</summary>
        public string? GuestName { get; set; }

        /// <summary>SĐT liên hệ</summary>
        public string? GuestPhone { get; set; }

        /// <summary>Email liên hệ</summary>
        public string? GuestEmail { get; set; }

        /// <summary>Số lượng khách dự kiến</summary>
        public int PartySize { get; set; }

        /// <summary>Thời điểm khách muốn đến</summary>
        public DateTime ReservedAt { get; set; }

        /// <summary>Ghi chú đặc biệt (VD: yêu cầu bàn gần cửa sổ)</summary>
        public string? Note { get; set; }

        /// <summary>Trạng thái: pending/confirmed/seated/completed/cancelled/no_show</summary>
        public string Status { get; set; } = ReservationStatus.Pending;

        /// <summary>Nhân viên xác nhận</summary>
        public Guid? ConfirmedByUserId { get; set; }
    }

    public static class ReservationStatus
    {
        public const string Pending = "pending";
        public const string Confirmed = "confirmed";
        public const string Seated = "seated";
        public const string Completed = "completed";
        public const string Cancelled = "cancelled";
        public const string NoShow = "no_show";
    }
}
