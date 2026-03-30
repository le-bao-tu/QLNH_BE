using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Employee.Models
{
    /// <summary>Thông tin nhân viên</summary>
    public class Employee : BaseEntity
    {
        /// <summary>Chi nhánh làm việc chính</summary>
        public Guid BranchId { get; set; }
        public Branch.Models.Branch? Branch { get; set; }

        /// <summary>Liên kết tài khoản đăng nhập</summary>
        public Guid? UserId { get; set; }

        /// <summary>Họ tên nhân viên</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Số điện thoại</summary>
        public string? Phone { get; set; }

        /// <summary>Email đăng nhập / liên hệ</summary>
        public string? Email { get; set; }

        /// <summary>Vai trò: owner / manager / cashier / waiter / chef / bartender</summary>
        public string Role { get; set; } = EmployeeRole.Waiter;

        /// <summary>Lương theo giờ</summary>
        public decimal HourlyRate { get; set; } = 0;

        /// <summary>Ngày bắt đầu làm việc</summary>
        public DateOnly? HiredAt { get; set; }

        /// <summary>URL ảnh đại diện</summary>
        public string? AvatarUrl { get; set; }

        // Navigation
        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }

    /// <summary>Ca làm việc của nhân viên</summary>
    public class Shift : BaseEntity
    {
        /// <summary>Chi nhánh làm ca</summary>
        public Guid BranchId { get; set; }
        public Branch.Models.Branch? Branch { get; set; }

        /// <summary>Nhân viên làm ca</summary>
        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        /// <summary>Ngày làm việc</summary>
        public DateOnly ShiftDate { get; set; }

        /// <summary>Giờ bắt đầu ca (dự kiến)</summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>Giờ kết thúc ca (dự kiến)</summary>
        public TimeOnly EndTime { get; set; }

        /// <summary>Thực tế giờ vào ca (chấm công)</summary>
        public DateTime? ActualStart { get; set; }

        /// <summary>Thực tế giờ kết thúc ca</summary>
        public DateTime? ActualEnd { get; set; }

        /// <summary>Trạng thái ca: scheduled / active / completed / absent</summary>
        public string Status { get; set; } = ShiftStatus.Scheduled;

        /// <summary>Ghi chú ca làm</summary>
        public string? Note { get; set; }
    }

    public static class EmployeeRole
    {
        public const string Owner = "owner";
        public const string Manager = "manager";
        public const string Cashier = "cashier";
        public const string Waiter = "waiter";
        public const string Chef = "chef";
        public const string Bartender = "bartender";
    }

    public static class ShiftStatus
    {
        public const string Scheduled = "scheduled";
        public const string Active = "active";
        public const string Completed = "completed";
        public const string Absent = "absent";
    }
}
