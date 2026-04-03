using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Table.DTOs;
using RestaurantApp.API.Modules.Table.Models;

namespace RestaurantApp.API.Modules.Table.Services
{
    public interface ITableService
    {
        Task<List<TableDto>> GetByBranchAsync(Guid branchId);
        Task<TableDto?> GetByIdAsync(Guid id);
        Task<TableDto> CreateAsync(CreateTableDto dto);
        Task<TableDto?> UpdateAsync(Guid id, UpdateTableDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<TableDto?> UpdateStatusAsync(Guid id, string status);
    }

    public class TableService : ITableService
    {
        private readonly AppDbContext _context;

        public TableService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TableDto>> GetByBranchAsync(Guid branchId)
        {
            var tables = await _context.Tables
                .Where(t => t.BranchId == branchId)
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            var activeOrders = await _context.Orders
                .Where(o => o.Status != Order.Models.OrderStatus.Paid && o.Status != Order.Models.OrderStatus.Cancelled)
                .Select(o => new { o.TableId, o.Id })
                .ToListAsync();

            return tables.Select(t =>
            {
                var currentOrder = activeOrders.FirstOrDefault(o => o.TableId == t.Id);
                return new TableDto
                {
                    Id = t.Id,
                    BranchId = t.BranchId,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity,
                    Status = t.Status,
                    Note = t.Note,
                    CreatedAt = t.CreatedAt,
                    CurrentOrderId = currentOrder?.Id
                };
            }).ToList();
        }

        public async Task<TableDto?> GetByIdAsync(Guid id)
        {
            var t = await _context.Tables.FindAsync(id);
            if (t == null) return null;

            return new TableDto
            {
                Id = t.Id,
                BranchId = t.BranchId,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Status = t.Status,
                Note = t.Note,
                CreatedAt = t.CreatedAt
            };
        }

        public async Task<TableDto> CreateAsync(CreateTableDto dto)
        {
            var tableId = Guid.NewGuid();
            var table = new Models.Table
            {
                Id = tableId,
                BranchId = dto.BranchId,
                TableNumber = dto.TableNumber,
                Capacity = dto.Capacity,
                Note = dto.Note,
                Status = TableStatus.Available,
                CreatedAt = DateTime.UtcNow,
                QrCode = $"https://pfxsoft-rms.com/order?tableId={tableId}" // Step 3.2
            };
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(table.Id) ?? throw new Exception("Không thể tạo bàn");
        }

        public async Task<TableDto?> UpdateAsync(Guid id, UpdateTableDto dto)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return null;

            if (dto.TableNumber.HasValue) table.TableNumber = dto.TableNumber.Value;
            if (dto.Capacity.HasValue) table.Capacity = dto.Capacity.Value;
            if (dto.Status != null) table.Status = dto.Status;
            if (dto.Note != null) table.Note = dto.Note;
            table.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return false;
            table.IsDeleted = true;
            table.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TableDto?> UpdateStatusAsync(Guid id, string status)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return null;
            table.Status = status;
            table.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }
    }
}
