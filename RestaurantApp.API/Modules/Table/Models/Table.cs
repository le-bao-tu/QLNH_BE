using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Table.Models
{
    /// <summary>Thông tin bàn ăn</summary>
    public class Table : BaseEntity
    {
        /// <summary>Chi nhánh sở hữu bàn</summary>
        public Guid BranchId { get; set; }
        public Guid? RestaurantId { get; set; }
        public Branch.Models.Branch? Branch { get; set; }

        /// <summary>Số bàn hiển thị</summary>
        public int TableNumber { get; set; }

        /// <summary>Sức chứa tối đa</summary>
        public int Capacity { get; set; }

        /// <summary>Trạng thái: available | occupied | reserved | cleaning</summary>
        public string Status { get; set; } = TableStatus.Available;

        /// <summary>Tầng hoặc khu vực đặt bàn</summary>
        public int? Floor { get; set; }

        /// <summary>Mã QR để khách tự order</summary>
        public string? QrCode { get; set; }

        /// <summary>Ghi chú bàn</summary>
        public string? Note { get; set; }

        // Navigation
        public ICollection<Order.Models.Order> Orders { get; set; } = new List<Order.Models.Order>();
        public ICollection<Reservation.Models.Reservation> Reservations { get; set; } = new List<Reservation.Models.Reservation>();
    }

    public static class TableStatus
    {
        public const string Available = "available";
        public const string Occupied = "occupied";
        public const string Reserved = "reserved";
        public const string Cleaning = "cleaning";
    }
}
