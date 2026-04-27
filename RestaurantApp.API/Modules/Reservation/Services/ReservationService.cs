using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Common;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Reservation.DTOs;
using RestaurantApp.API.Modules.Reservation.Models;
using RestaurantApp.API.Modules.Table.Models;

namespace RestaurantApp.API.Modules.Reservation.Services
{
    public interface IReservationService
    {
        Task<List<ReservationDto>> GetByBranchAsync(Guid branchId, DateOnly? date = null);
        Task<PagedResult<ReservationDto>> GetByBranchPagedAsync(Guid branchId, DateOnly? date, PaginationParams @params);
        Task<ReservationDto?> GetByIdAsync(Guid id);
        Task<ReservationDto> CreateAsync(CreateReservationDto dto);
        Task<ReservationDto?> UpdateStatusAsync(Guid id, UpdateReservationStatusDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id);
    }

    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;

        public ReservationService(AppDbContext context) => _context = context;

        public async Task<List<ReservationDto>> GetByBranchAsync(Guid branchId, DateOnly? date = null)
        {
            var query = _context.Reservations
                .Include(r => r.Table)
                .Include(r => r.Customer)
                .Where(r => r.BranchId == branchId && !r.IsDeleted);

            if (date.HasValue)
            {
                var start = date.Value.ToDateTime(TimeOnly.MinValue);
                var end = date.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(r => r.ReservedAt >= start && r.ReservedAt <= end);
            }

            return await query
                .OrderBy(r => r.ReservedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }

        public async Task<PagedResult<ReservationDto>> GetByBranchPagedAsync(Guid branchId, DateOnly? date, PaginationParams @params)
        {
            var query = _context.Reservations
                .Include(r => r.Table)
                .Include(r => r.Customer)
                .Where(r => r.BranchId == branchId && !r.IsDeleted);

            if (date.HasValue)
            {
                var start = date.Value.ToDateTime(TimeOnly.MinValue);
                var end = date.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(r => r.ReservedAt >= start && r.ReservedAt <= end);
            }

            if (!string.IsNullOrWhiteSpace(@params.Search))
            {
                query = query.Where(r => 
                    r.GuestName.Contains(@params.Search) || 
                    r.GuestPhone.Contains(@params.Search) || 
                    (r.Customer != null && r.Customer.FullName.Contains(@params.Search)));
            }

            return await query
                .OrderBy(r => r.ReservedAt)
                .Select(r => MapToDto(r))
                .ToPagedResultAsync(@params.PageIndex, @params.PageSize);
        }

        public async Task<ReservationDto?> GetByIdAsync(Guid id)
        {
            var r = await _context.Reservations
                .Include(r => r.Table)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            return r == null ? null : MapToDto(r);
        }

        public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
        {
            var reservation = new Models.Reservation
            {
                BranchId = dto.BranchId,
                TableId = dto.TableId,
                CustomerId = dto.CustomerId,
                GuestName = dto.GuestName,
                GuestPhone = dto.GuestPhone,
                GuestEmail = dto.GuestEmail,
                PartySize = dto.PartySize,
                ReservedAt = dto.ReservedAt,
                Note = dto.Note,
                Status = ReservationStatus.Confirmed,
            };

            _context.Reservations.Add(reservation);

            // Mark table as reserved if specified
            if (dto.TableId.HasValue)
            {
                var table = await _context.Tables.FindAsync(dto.TableId.Value);
                if (table != null && table.Status == TableStatus.Available)
                {
                    table.Status = TableStatus.Reserved;
                    table.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(reservation.Id) ?? throw new Exception("Lỗi tạo đặt bàn");
        }

        public async Task<ReservationDto?> UpdateStatusAsync(Guid id, UpdateReservationStatusDto dto, Guid userId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return null;

            reservation.Status = dto.Status;
            reservation.ConfirmedByUserId = userId;
            reservation.UpdatedAt = DateTime.UtcNow;

            // Assign table on seat
            if (dto.Status == ReservationStatus.Seated && dto.TableId.HasValue)
            {
                reservation.TableId = dto.TableId.Value;
                var table = await _context.Tables.FindAsync(dto.TableId.Value);
                if (table != null)
                {
                    table.Status = TableStatus.Occupied;
                    table.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Free table on cancel/no_show
            if ((dto.Status == ReservationStatus.Cancelled || dto.Status == ReservationStatus.NoShow)
                && reservation.TableId.HasValue)
            {
                var table = await _context.Tables.FindAsync(reservation.TableId.Value);
                if (table != null && table.Status == TableStatus.Reserved)
                {
                    table.Status = TableStatus.Available;
                    table.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r == null) return false;
            r.IsDeleted = true;
            r.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static ReservationDto MapToDto(Models.Reservation r) => new()
        {
            Id = r.Id,
            BranchId = r.BranchId,
            TableId = r.TableId,
            TableNumber = r.Table?.TableNumber,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.FullName,
            GuestName = r.GuestName,
            GuestPhone = r.GuestPhone,
            GuestEmail = r.GuestEmail,
            PartySize = r.PartySize,
            ReservedAt = r.ReservedAt,
            Note = r.Note,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
        };
    }
}
