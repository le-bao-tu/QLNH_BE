namespace RestaurantApp.API.Modules.Reservation.DTOs
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid? TableId { get; set; }
        public int? TableNumber { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public int PartySize { get; set; }
        public DateTime ReservedAt { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReservationDto
    {
        public Guid BranchId { get; set; }
        public Guid? TableId { get; set; }
        public Guid? CustomerId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public int PartySize { get; set; }
        public DateTime ReservedAt { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateReservationStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public Guid? TableId { get; set; }
    }
}
