using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Inventory.Models;
using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Inventory.Services
{
    public record SupplierDto(Guid Id, Guid RestaurantId, string Name, string? ContactName, string? Phone, string? Email, string? Address, string? Note);
    public record InventoryItemDto(Guid Id, Guid BranchId, Guid? SupplierId, string? SupplierName, string Name, string Unit, decimal CurrentQuantity, decimal MinQuantity, decimal CostPrice, bool IsLowStock);
    public record InventoryTxDto(Guid Id, string Type, decimal QuantityChange, decimal QuantityAfter, string? Note, DateTime CreatedAt);
    public record CreateSupplierDto(Guid RestaurantId, string Name, string? ContactName, string? Phone, string? Email, string? Address, string? Note);
    public record CreateInventoryItemDto(Guid BranchId, Guid? SupplierId, string Name, string Unit, decimal MinQuantity, decimal CostPrice);
    public record InventoryTransactionDto(Guid InventoryItemId, string Type, decimal QuantityChange, string? Note, Guid? PerformedBy);

    public interface IInventoryService
    {
        Task<List<SupplierDto>> GetSuppliersAsync(Guid restaurantId);
        Task<PagedResult<SupplierDto>> GetSuppliersPagedAsync(Guid restaurantId, PaginationParams @params);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto);
        Task<List<InventoryItemDto>> GetItemsByBranchAsync(Guid branchId);
        Task<PagedResult<InventoryItemDto>> GetItemsByBranchPagedAsync(Guid branchId, bool? lowStockOnly, PaginationParams @params);
        Task<List<InventoryItemDto>> GetItemsByRestaurantAsync(Guid restaurantId);
        Task<PagedResult<InventoryItemDto>> GetItemsByRestaurantPagedAsync(Guid restaurantId, bool? lowStockOnly, PaginationParams @params);
        Task<InventoryItemDto?> GetItemByIdAsync(Guid id);
        Task<InventoryItemDto> CreateItemAsync(CreateInventoryItemDto dto);
        Task<InventoryItemDto?> UpdateItemAsync(Guid id, CreateInventoryItemDto dto);
        Task<InventoryTxDto> RecordTransactionAsync(InventoryTransactionDto dto);
        Task<List<InventoryItemDto>> GetLowStockItemsAsync(Guid branchId);
        Task<List<InventoryItemDto>> GetLowStockItemsByRestaurantAsync(Guid restaurantId);
    }

    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _ctx;
        public InventoryService(AppDbContext ctx) => _ctx = ctx;

        public async Task<List<SupplierDto>> GetSuppliersAsync(Guid restaurantId)
            => await _ctx.Suppliers.Where(s => s.RestaurantId == restaurantId)
                .Select(s => ToSupDto(s)).ToListAsync();

        public async Task<PagedResult<SupplierDto>> GetSuppliersPagedAsync(Guid restaurantId, PaginationParams @params)
        {
            var query = _ctx.Suppliers.Where(s => s.RestaurantId == restaurantId);
            if (!string.IsNullOrWhiteSpace(@params.Search))
                query = query.Where(s => s.Name.Contains(@params.Search));
            
            return await query.OrderBy(s => s.Name)
                .Select(s => ToSupDto(s))
                .ToPagedResultAsync(@params.PageIndex, @params.PageSize);
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
        {
            var sup = new Supplier { RestaurantId = dto.RestaurantId, Name = dto.Name, ContactName = dto.ContactName, Phone = dto.Phone, Email = dto.Email, Address = dto.Address, Note = dto.Note };
            _ctx.Suppliers.Add(sup);
            await _ctx.SaveChangesAsync();
            return ToSupDto(sup);
        }

        public async Task<List<InventoryItemDto>> GetItemsByBranchAsync(Guid branchId)
            => await _ctx.InventoryItems.Include(i => i.Supplier).Where(i => i.BranchId == branchId)
                .OrderBy(i => i.Name).Select(i => ToItemDto(i)).ToListAsync();

        public async Task<PagedResult<InventoryItemDto>> GetItemsByBranchPagedAsync(Guid branchId, bool? lowStockOnly, PaginationParams @params)
        {
            var query = _ctx.InventoryItems.Include(i => i.Supplier).Where(i => i.BranchId == branchId);
            if (!string.IsNullOrWhiteSpace(@params.Search))
                query = query.Where(i => i.Name.Contains(@params.Search));
            
            if (lowStockOnly == true)
                query = query.Where(i => i.CurrentQuantity <= i.MinQuantity);
            
            return await query.OrderBy(i => i.Name)
                .Select(i => ToItemDto(i))
                .ToPagedResultAsync(@params.PageIndex, @params.PageSize);
        }

        public async Task<List<InventoryItemDto>> GetItemsByRestaurantAsync(Guid restaurantId)
            => await _ctx.InventoryItems.Include(i => i.Supplier).Include(i => i.Branch)
                .Where(i => i.Branch!.RestaurantId == restaurantId)
                .OrderBy(i => i.Name).Select(i => ToItemDto(i)).ToListAsync();

        public async Task<PagedResult<InventoryItemDto>> GetItemsByRestaurantPagedAsync(Guid restaurantId, bool? lowStockOnly, PaginationParams @params)
        {
            var query = _ctx.InventoryItems.Include(i => i.Supplier).Include(i => i.Branch)
                .Where(i => i.Branch!.RestaurantId == restaurantId);
            if (!string.IsNullOrWhiteSpace(@params.Search))
                query = query.Where(i => i.Name.Contains(@params.Search));

            if (lowStockOnly == true)
                query = query.Where(i => i.CurrentQuantity <= i.MinQuantity);

            return await query.OrderBy(i => i.Name)
                .Select(i => ToItemDto(i))
                .ToPagedResultAsync(@params.PageIndex, @params.PageSize);
        }

        public async Task<InventoryItemDto?> GetItemByIdAsync(Guid id)
        {
            var i = await _ctx.InventoryItems.Include(ii => ii.Supplier).FirstOrDefaultAsync(ii => ii.Id == id);
            return i == null ? null : ToItemDto(i);
        }

        public async Task<InventoryItemDto> CreateItemAsync(CreateInventoryItemDto dto)
        {
            var item = new InventoryItem { BranchId = dto.BranchId, SupplierId = dto.SupplierId, Name = dto.Name, Unit = dto.Unit, MinQuantity = dto.MinQuantity, CostPrice = dto.CostPrice };
            _ctx.InventoryItems.Add(item);
            await _ctx.SaveChangesAsync();
            return await GetItemByIdAsync(item.Id) ?? throw new Exception("Lỗi");
        }

        public async Task<InventoryItemDto?> UpdateItemAsync(Guid id, CreateInventoryItemDto dto)
        {
            var item = await _ctx.InventoryItems.FindAsync(id);
            if (item == null) return null;
            item.SupplierId = dto.SupplierId; item.Name = dto.Name; item.Unit = dto.Unit;
            item.MinQuantity = dto.MinQuantity; item.CostPrice = dto.CostPrice;
            item.UpdatedAt = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
            return await GetItemByIdAsync(id);
        }

        public async Task<InventoryTxDto> RecordTransactionAsync(InventoryTransactionDto dto)
        {
            var item = await _ctx.InventoryItems.FindAsync(dto.InventoryItemId) ?? throw new Exception("Không tìm thấy nguyên liệu");
            item.CurrentQuantity += dto.QuantityChange;
            item.UpdatedAt = DateTime.UtcNow;

            var tx = new InventoryTransaction
            {
                InventoryItemId = dto.InventoryItemId, Type = dto.Type,
                QuantityChange = dto.QuantityChange, QuantityAfter = item.CurrentQuantity,
                Note = dto.Note, PerformedBy = dto.PerformedBy,
            };
            _ctx.InventoryTransactions.Add(tx);
            await _ctx.SaveChangesAsync();
            return new InventoryTxDto(tx.Id, tx.Type, tx.QuantityChange, tx.QuantityAfter, tx.Note, tx.CreatedAt);
        }

        public async Task<List<InventoryItemDto>> GetLowStockItemsAsync(Guid branchId)
            => await _ctx.InventoryItems.Include(i => i.Supplier)
                .Where(i => i.BranchId == branchId && i.CurrentQuantity <= i.MinQuantity)
                .Select(i => ToItemDto(i)).ToListAsync();

        public async Task<List<InventoryItemDto>> GetLowStockItemsByRestaurantAsync(Guid restaurantId)
            => await _ctx.InventoryItems.Include(i => i.Supplier).Include(i => i.Branch)
                .Where(i => i.Branch!.RestaurantId == restaurantId && i.CurrentQuantity <= i.MinQuantity)
                .Select(i => ToItemDto(i)).ToListAsync();

        private static SupplierDto ToSupDto(Supplier s) => new(s.Id, s.RestaurantId, s.Name, s.ContactName, s.Phone, s.Email, s.Address, s.Note);
        private static InventoryItemDto ToItemDto(InventoryItem i) => new(i.Id, i.BranchId, i.SupplierId, i.Supplier?.Name, i.Name, i.Unit, i.CurrentQuantity, i.MinQuantity, i.CostPrice, i.CurrentQuantity <= i.MinQuantity);
    }
}
