namespace RestaurantApp.API.Modules.Table.DTOs
{
    public class CreateTableDto
    {
        public Guid BranchId { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateTableDto
    {
        public int? TableNumber { get; set; }
        public int? Capacity { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }

    public class TableDto
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        // Thông tin đơn hàng hiện tại (nếu có)
        public Guid? CurrentOrderId { get; set; }
    }
}
