using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Employee.Models;

namespace RestaurantApp.API.Modules.Employee.Services
{
    public record EmployeeDto(Guid Id, Guid BranchId, string FullName, string? Phone, string? Email, Guid? RoleId, decimal HourlyRate, DateOnly? HiredAt, string? AvatarUrl, DateTime CreatedAt);
    public record CreateEmployeeDto(Guid BranchId, string FullName, string? Phone, string? Email, Guid? RoleId, decimal HourlyRate, DateOnly? HiredAt, string? Username = null, string? Password = null);
    public record ShiftDto(Guid Id, Guid EmployeeId, string EmployeeName, DateOnly ShiftDate, TimeOnly StartTime, TimeOnly EndTime, DateTime? ActualStart, DateTime? ActualEnd, string Status, string? Note);
    public record CreateShiftDto(Guid BranchId, Guid EmployeeId, DateOnly ShiftDate, TimeOnly StartTime, TimeOnly EndTime, string? Note);

    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetByBranchAsync(Guid branchId);
        Task<EmployeeDto?> GetByIdAsync(Guid id);
        Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
        Task<EmployeeDto?> UpdateAsync(Guid id, CreateEmployeeDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<ShiftDto>> GetShiftsAsync(Guid branchId, DateOnly? date = null);
        Task<ShiftDto> CreateShiftAsync(CreateShiftDto dto);
        Task<ShiftDto?> CheckInAsync(Guid shiftId);
        Task<ShiftDto?> CheckOutAsync(Guid shiftId);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _ctx;
        public EmployeeService(AppDbContext ctx) => _ctx = ctx;

        public async Task<List<EmployeeDto>> GetByBranchAsync(Guid branchId)
            => await _ctx.Employees.Where(e => e.BranchId == branchId)
                .OrderBy(e => e.FullName).Select(e => ToDto(e)).ToListAsync();

        public async Task<EmployeeDto?> GetByIdAsync(Guid id)
        {
            var e = await _ctx.Employees.FindAsync(id);
            return e == null ? null : ToDto(e);
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
        {
            var e = new Models.Employee
            {
                Id = Guid.NewGuid(),
                BranchId = dto.BranchId, FullName = dto.FullName, Phone = dto.Phone,
                Email = dto.Email, RoleId = dto.RoleId!.Value, HourlyRate = dto.HourlyRate, HiredAt = dto.HiredAt,
                CreatedAt = DateTime.UtcNow
            };
            _ctx.Employees.Add(e);

            // Create User Account if provided (Step 2.2)
            if (!string.IsNullOrEmpty(dto.Username) && !string.IsNullOrEmpty(dto.Password))
            {
                var user = new RestaurantApp.API.Modules.Auth.Models.User
                {
                    Id = Guid.NewGuid(),
                    Username = dto.Username,
                    Email = dto.Email ?? $"{dto.Username}@restaurant.com",
                    FullName = dto.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = dto.RoleId!.Value
                };
                _ctx.Users.Add(user);
            }

            await _ctx.SaveChangesAsync();
            return ToDto(e);
        }

        public async Task<EmployeeDto?> UpdateAsync(Guid id, CreateEmployeeDto dto)
        {
            var e = await _ctx.Employees.FindAsync(id);
            if (e == null) return null;
            e.FullName = dto.FullName; e.Phone = dto.Phone; e.Email = dto.Email;
            e.RoleId = dto.RoleId!.Value; e.HourlyRate = dto.HourlyRate; e.HiredAt = dto.HiredAt;
            e.UpdatedAt = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
            return ToDto(e);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var e = await _ctx.Employees.FindAsync(id);
            if (e == null) return false;
            e.IsDeleted = true; e.UpdatedAt = DateTime.UtcNow;
            await _ctx.SaveChangesAsync(); return true;
        }

        public async Task<List<ShiftDto>> GetShiftsAsync(Guid branchId, DateOnly? date = null)
        {
            var query = _ctx.Shifts.Include(s => s.Employee).Where(s => s.BranchId == branchId);
            if (date.HasValue) query = query.Where(s => s.ShiftDate == date.Value);
            return await query.OrderBy(s => s.ShiftDate).ThenBy(s => s.StartTime)
                .Select(s => ToShiftDto(s)).ToListAsync();
        }

        public async Task<ShiftDto> CreateShiftAsync(CreateShiftDto dto)
        {
            var shift = new Models.Shift
            {
                BranchId = dto.BranchId, EmployeeId = dto.EmployeeId,
                ShiftDate = dto.ShiftDate, StartTime = dto.StartTime,
                EndTime = dto.EndTime, Note = dto.Note,
            };
            _ctx.Shifts.Add(shift);
            await _ctx.SaveChangesAsync();
            return await _ctx.Shifts.Include(s => s.Employee).Where(s => s.Id == shift.Id)
                .Select(s => ToShiftDto(s)).FirstAsync();
        }

        public async Task<ShiftDto?> CheckInAsync(Guid shiftId)
        {
            var shift = await _ctx.Shifts.Include(s => s.Employee).FirstOrDefaultAsync(s => s.Id == shiftId);
            if (shift == null) return null;
            shift.ActualStart = DateTime.UtcNow;
            shift.Status = ShiftStatus.Active;
            await _ctx.SaveChangesAsync();
            return ToShiftDto(shift);
        }

        public async Task<ShiftDto?> CheckOutAsync(Guid shiftId)
        {
            var shift = await _ctx.Shifts.Include(s => s.Employee).FirstOrDefaultAsync(s => s.Id == shiftId);
            if (shift == null) return null;
            shift.ActualEnd = DateTime.UtcNow;
            shift.Status = ShiftStatus.Completed;
            await _ctx.SaveChangesAsync();
            return ToShiftDto(shift);
        }

        private static EmployeeDto ToDto(Models.Employee e) => new(e.Id, e.BranchId, e.FullName, e.Phone, e.Email, e.RoleId, e.HourlyRate, e.HiredAt, e.AvatarUrl, e.CreatedAt);
        private static ShiftDto ToShiftDto(Models.Shift s) => new(s.Id, s.EmployeeId, s.Employee?.FullName ?? "", s.ShiftDate, s.StartTime, s.EndTime, s.ActualStart, s.ActualEnd, s.Status, s.Note);
    }
}
