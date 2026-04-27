using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Common;
using System.Linq;
using System.Collections.Generic;
using RestaurantApp.API.Modules.Auth.Models;
using RestaurantApp.API.Modules.Restaurant.Models;
using RestaurantApp.API.Modules.Branch.Models;
using RestaurantApp.API.Modules.Table.Models;
using RestaurantApp.API.Modules.Reservation.Models;
using RestaurantApp.API.Modules.Menu.Models;
using RestaurantApp.API.Modules.Order.Models;
using RestaurantApp.API.Modules.Payment.Models;
using RestaurantApp.API.Modules.Customer.Models;
using RestaurantApp.API.Modules.Employee.Models;
using RestaurantApp.API.Modules.Inventory.Models;
using RestaurantApp.API.Modules.Promotion.Models;
using RestaurantApp.API.Modules.Notification.Models;
using RestaurantApp.API.Modules.Audit.Models;
using RestaurantApp.API.Modules.Role.Models;

namespace RestaurantApp.API.Data
{
    public class AppDbContext : DbContext
    {
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(options) 
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            
            // Fix PostgreSQL DateTime Kind Issue Globally before save (Last Defense)
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var properties = entry.Entity.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var prop in properties)
                {
                    var val = prop.GetValue(entry.Entity);
                    if (val is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                    {
                        prop.SetValue(entry.Entity, DateTime.SpecifyKind(dt, DateTimeKind.Utc));
                    }
                }
            }

            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("nameid")?.Value;
                
                var branchIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("branchId")?.Value;
                var restaurantIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("restaurantId")?.Value;
                
                Guid? userId = Guid.TryParse(userIdStr, out var g) ? g : null;
                Guid? branchId = Guid.TryParse(branchIdStr, out var b) ? b : null;
                Guid? restaurantId = Guid.TryParse(restaurantIdStr, out var r) ? r : null;

                var auditEntry = new AuditEntry(entry)
                {
                    UserId = userId,
                    BranchId = branchId,
                    RestaurantId = restaurantId,
                    Module = entry.Entity.GetType().Name,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()
                };
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    
                    // Bỏ qua các trường nhạy cảm
                    if (propertyName.Contains("PasswordHash") || propertyName.Contains("Token"))
                        continue;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = "Tạo mới";
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = "Xoá";
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.AuditType = "Cập nhật";
                                auditEntry.OldValues[propertyName] = property.OriginalValue!;
                                auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                }
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            return base.SaveChangesAsync();
        }

        private class AuditEntry
        {
            public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
            {
                Entry = entry;
            }

            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
            public Guid? UserId { get; set; }
            public Guid? BranchId { get; set; }
            public Guid? RestaurantId { get; set; }
            public string Module { get; set; } = string.Empty;
            public string AuditType { get; set; } = string.Empty;
            public string? IpAddress { get; set; }
            public string? UserAgent { get; set; }
            public Dictionary<string, object> KeyValues { get; } = new();
            public Dictionary<string, object> OldValues { get; } = new();
            public Dictionary<string, object> NewValues { get; } = new();
            public List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new();

            public bool HasTemporaryProperties => TemporaryProperties.Any();

            public AuditLog ToAuditLog()
            {
                var audit = new AuditLog();
                audit.UserId = UserId;
                audit.BranchId = BranchId;
                audit.RestaurantId = RestaurantId;
                audit.Action = AuditType;
                audit.Module = Module;
                audit.CreatedAt = DateTime.UtcNow;
                audit.IpAddress = IpAddress;
                audit.UserAgent = UserAgent;
                audit.TargetId = KeyValues.Count > 0 ? (Guid?)KeyValues.Values.First() : null;
                audit.OldData = OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(OldValues);
                audit.NewData = NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(NewValues);
                return audit;
            }
        }

        // ===== AUTH =====
        public DbSet<User> Users => Set<User>();

        // ===== RESTAURANT & BRANCH =====
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<Branch> Branches => Set<Branch>();

        // ===== TABLES & RESERVATIONS =====
        public DbSet<Table> Tables => Set<Table>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

        // ===== MENU =====
        public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<MenuItemOptionGroup> MenuItemOptionGroups => Set<MenuItemOptionGroup>();
        public DbSet<MenuItemOption> MenuItemOptions => Set<MenuItemOption>();
        public DbSet<MenuItemCombo> MenuItemCombos => Set<MenuItemCombo>();

        // ===== ORDERS =====
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderItemOption> OrderItemOptions => Set<OrderItemOption>();
        public DbSet<TableTransfer> TableTransfers => Set<TableTransfer>();
        public DbSet<KitchenOrder> KitchenOrders => Set<KitchenOrder>();

        // ===== PAYMENT =====
        public DbSet<Payment> Payments => Set<Payment>();

        // ===== CUSTOMERS & LOYALTY =====
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();

        // ===== EMPLOYEES & SHIFTS =====
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Shift> Shifts => Set<Shift>();

        // ===== INVENTORY =====
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<MenuItemRecipe> MenuItemRecipes => Set<MenuItemRecipe>();

        // ===== PROMOTIONS =====
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<VoucherCode> VoucherCodes => Set<VoucherCode>();

        // ===== NOTIFICATIONS & AUDIT =====
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        
        // ===== ROLES =====
        public DbSet<RestaurantRole> RestaurantRoles => Set<RestaurantRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define converters outside the loop for efficiency
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            // ===== GLOBAL SOFT DELETE FILTER & DATETIME UTC CONVERTER =====
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Soft Delete
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(ConvertFilterExpression<BaseEntity>(e => !e.IsDeleted, entityType.ClrType));
                }

                // DateTime to UTC conversion (Fix for PostgreSQL)
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(DateTime))
                    {
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(property.Name)
                            .HasConversion(dateTimeConverter);
                    }
                    else if (property.PropertyType == typeof(DateTime?))
                    {
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(property.Name)
                            .HasConversion(nullableDateTimeConverter);
                    }
                }
            }

            // ===== USERS =====
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasIndex(x => x.Username).IsUnique().HasDatabaseName("idx_users_username");
                e.Property(x => x.Username).HasComment("Tên đăng nhập");
                e.Property(x => x.Email).HasComment("Email đăng nhập");
                e.Property(x => x.RoleId).HasComment("Vai trò: Admin/Manager/Staff");
            });

            // ===== RESTAURANTS =====
            modelBuilder.Entity<Restaurant>(e =>
            {
                e.ToTable("restaurants");
                e.Property(x => x.Name).HasComment("Tên nhà hàng");
                e.Property(x => x.OwnerId).HasComment("ID chủ nhà hàng");
                e.Property(x => x.LogoUrl).HasComment("Đường dẫn logo nhà hàng");
                e.Property(x => x.TaxCode).HasComment("Mã số thuế doanh nghiệp");
            });

            // ===== BRANCHES =====
            modelBuilder.Entity<Branch>(e =>
            {
                e.ToTable("branches");
                e.HasIndex(x => x.RestaurantId).HasDatabaseName("idx_branches_restaurant_id");
                e.Property(x => x.Name).HasComment("Tên chi nhánh");
                e.Property(x => x.Address).HasComment("Địa chỉ chi nhánh");
                e.Property(x => x.Phone).HasComment("Số điện thoại chi nhánh");
                e.HasOne(b => b.Restaurant)
                 .WithMany(r => r.Branches)
                 .HasForeignKey(b => b.RestaurantId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== TABLES =====
            modelBuilder.Entity<Table>(e =>
            {
                e.ToTable("tables");
                e.HasIndex(x => x.BranchId).HasDatabaseName("idx_tables_branch_id");
                e.HasIndex(x => new { x.BranchId, x.TableNumber }).IsUnique().HasDatabaseName("idx_tables_branch_number");
                e.Property(x => x.TableNumber).HasComment("Số bàn hiển thị");
                e.Property(x => x.Capacity).HasComment("Sức chứa tối đa");
                e.Property(x => x.Status).HasComment("Trạng thái: available/occupied/reserved/cleaning");
                e.Property(x => x.Floor).HasComment("Tầng hoặc khu vực đặt bàn");
                e.Property(x => x.QrCode).HasComment("Mã QR để khách tự order");
                e.HasOne(t => t.Branch)
                 .WithMany(b => b.Tables)
                 .HasForeignKey(t => t.BranchId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== RESERVATIONS =====
            modelBuilder.Entity<Reservation>(e =>
            {
                e.ToTable("reservations");
                e.HasIndex(x => x.BranchId).HasDatabaseName("idx_reservations_branch_id");
                e.HasIndex(x => x.ReservedAt).HasDatabaseName("idx_reservations_reserved_at");
                e.Property(x => x.PartySize).HasComment("Số lượng khách dự kiến");
                e.Property(x => x.ReservedAt).HasComment("Thời điểm khách muốn đến");
                e.Property(x => x.Status).HasComment("pending/confirmed/seated/completed/cancelled/no_show");
                e.HasOne(r => r.Branch).WithMany().HasForeignKey(r => r.BranchId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Table).WithMany(t => t.Reservations).HasForeignKey(r => r.TableId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.SetNull);
            });

            // ===== MENU CATEGORIES =====
            modelBuilder.Entity<MenuCategory>(e =>
            {
                e.ToTable("menu_categories");
                e.HasIndex(x => x.RestaurantId).HasDatabaseName("idx_menu_categories_restaurant_id");
                e.Property(x => x.BranchIds).HasComment("JSON array of branch ids. NULL = áp dụng cho tất cả chi nhánh");
                e.Property(x => x.SortOrder).HasComment("Thứ tự hiển thị danh mục trên menu");
                e.Property(x => x.IsActive).HasComment("Danh mục đang được hiển thị hay ẩn");
            });

            // ===== MENU ITEMS =====
            modelBuilder.Entity<MenuItem>(e =>
            {
                e.ToTable("menu_items");
                e.HasIndex(x => x.CategoryId).HasDatabaseName("idx_menu_items_category_id");
                e.Property(x => x.BasePrice).HasColumnType("numeric(12,2)").HasComment("Giá mặc định của món");
                e.Property(x => x.BranchIds).HasComment("JSON array of branch ids. NULL = áp dụng cho tất cả chi nhánh");
                e.Property(x => x.IsAvailable).HasComment("FALSE khi món tạm hết hoặc không phục vụ");
                e.HasOne(m => m.Category).WithMany(c => c.MenuItems).HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            // ===== MENU ITEM OPTION GROUPS =====
            modelBuilder.Entity<MenuItemOptionGroup>(e =>
            {
                e.ToTable("menu_item_option_groups");
                e.Property(x => x.IsRequired).HasComment("TRUE = khách bắt buộc phải chọn");
                e.Property(x => x.IsMultiple).HasComment("TRUE = khách được chọn nhiều option cùng lúc");
                e.HasOne(g => g.MenuItem).WithMany(m => m.OptionGroups).HasForeignKey(g => g.MenuItemId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== MENU ITEM OPTIONS =====
            modelBuilder.Entity<MenuItemOption>(e =>
            {
                e.ToTable("menu_item_options");
                e.Property(x => x.ExtraPrice).HasColumnType("numeric(12,2)").HasComment("Giá cộng thêm so với giá gốc của món");
                e.HasOne(o => o.OptionGroup).WithMany(g => g.Options).HasForeignKey(o => o.OptionGroupId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== MENU ITEM COMBOS =====
            modelBuilder.Entity<MenuItemCombo>(e =>
            {
                e.ToTable("menu_item_combos");
                e.HasIndex(x => x.ComboItemId).HasDatabaseName("idx_menu_item_combos_combo_id");
                e.HasIndex(x => x.SingleItemId).HasDatabaseName("idx_menu_item_combos_single_id");
                
                e.HasOne(c => c.ComboItem)
                 .WithMany(m => m.ComboItems)
                 .HasForeignKey(c => c.ComboItemId)
                 .OnDelete(DeleteBehavior.Cascade);
                 
                e.HasOne(c => c.SingleItem)
                 .WithMany()
                 .HasForeignKey(c => c.SingleItemId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ORDERS =====
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("orders");
                e.HasIndex(x => x.BranchId).HasDatabaseName("idx_orders_branch_id");
                e.HasIndex(x => x.TableId).HasDatabaseName("idx_orders_table_id");
                e.HasIndex(x => x.Status).HasDatabaseName("idx_orders_status");
                e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_orders_created_at");
                e.Property(x => x.Subtotal).HasColumnType("numeric(12,2)").HasComment("Tổng tiền trước giảm giá");
                e.Property(x => x.DiscountAmount).HasColumnType("numeric(12,2)").HasComment("Số tiền được giảm");
                e.Property(x => x.TaxAmount).HasColumnType("numeric(12,2)").HasComment("Thuế VAT");
                e.Property(x => x.TotalAmount).HasColumnType("numeric(12,2)").HasComment("Tổng tiền phải trả");
                e.Property(x => x.Status).HasComment("pending/preparing/ready/served/paid/cancelled");
                e.HasOne(o => o.Table).WithMany(t => t.Orders).HasForeignKey(o => o.TableId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(o => o.Branch).WithMany().HasForeignKey(o => o.BranchId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.SetNull);
            });

            // ===== ORDER ITEMS =====
            modelBuilder.Entity<OrderItem>(e =>
            {
                e.ToTable("order_items");
                e.HasIndex(x => x.OrderId).HasDatabaseName("idx_order_items_order_id");
                e.Property(x => x.UnitPrice).HasColumnType("numeric(12,2)").HasComment("Giá tại thời điểm order, không thay đổi dù menu cập nhật sau");
                e.Property(x => x.TotalPrice).HasColumnType("numeric(12,2)").HasComment("Thành tiền");
                e.Property(x => x.SentToKitchenAt).HasComment("Thời điểm nhân viên gửi món vào bếp");
                e.Property(x => x.Status).HasComment("pending/cooking/ready/served/cancelled");
                e.HasOne(oi => oi.Order).WithMany(o => o.OrderItems).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(oi => oi.MenuItem).WithMany().HasForeignKey(oi => oi.MenuItemId).OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ORDER ITEM OPTIONS =====
            modelBuilder.Entity<OrderItemOption>(e =>
            {
                e.ToTable("order_item_options");
                e.Property(x => x.ExtraPrice).HasColumnType("numeric(12,2)").HasComment("Giá thêm snapshot tại thời điểm order");
                e.HasOne(o => o.OrderItem).WithMany(oi => oi.SelectedOptions).HasForeignKey(o => o.OrderItemId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== TABLE TRANSFERS =====
            modelBuilder.Entity<TableTransfer>(e =>
            {
                e.ToTable("table_transfers");
                e.Property(x => x.TransferredAt).HasComment("Thời điểm chuyển bàn");
                e.HasOne(tt => tt.Order).WithMany(o => o.TableTransfers).HasForeignKey(tt => tt.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== KITCHEN ORDERS =====
            modelBuilder.Entity<KitchenOrder>(e =>
            {
                e.ToTable("kitchen_orders");
                e.HasIndex(x => new { x.BranchId, x.Status }).HasDatabaseName("idx_kitchen_orders_branch_status");
                e.Property(x => x.TableNumber).HasComment("Snapshot số bàn để hiển thị nhanh trên KDS");
                e.Property(x => x.Priority).HasComment("Số càng cao càng ưu tiên chế biến trước");
                e.Property(x => x.ReceivedAt).HasComment("Thời điểm ticket xuất hiện ở bếp");
                e.HasOne(ko => ko.OrderItem).WithOne(oi => oi.KitchenOrder)
                 .HasForeignKey<KitchenOrder>(ko => ko.OrderItemId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== PAYMENTS =====
            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("payments");
                e.HasIndex(x => x.OrderId).HasDatabaseName("idx_payments_order_id");
                e.Property(x => x.Amount).HasColumnType("numeric(12,2)").HasComment("Số tiền thanh toán lần này");
                e.Property(x => x.Method).HasComment("Phương thức: cash/momo/zalopay/vnpay/credit_card...");
                e.Property(x => x.ReferenceCode).HasComment("Mã giao dịch từ cổng thanh toán bên ngoài");
                e.HasOne(p => p.Order).WithMany().HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CUSTOMERS =====
            modelBuilder.Entity<Customer>(e =>
            {
                e.ToTable("customers");
                e.HasIndex(x => new { x.RestaurantId, x.Phone }).HasDatabaseName("idx_customers_restaurant_phone");
                e.Property(x => x.LoyaltyPoints).HasComment("Điểm tích lũy hiện tại");
                e.Property(x => x.LoyaltyTier).HasComment("bronze/silver/gold/platinum");
            });

            // ===== LOYALTY TRANSACTIONS =====
            modelBuilder.Entity<LoyaltyTransaction>(e =>
            {
                e.ToTable("loyalty_transactions");
                e.HasIndex(x => x.CustomerId).HasDatabaseName("idx_loyalty_transactions_customer_id");
                e.Property(x => x.PointsChange).HasComment("Số điểm thay đổi (dương = cộng, âm = trừ)");
                e.Property(x => x.BalanceAfter).HasComment("Điểm còn lại sau giao dịch");
                e.HasOne(lt => lt.Customer).WithMany(c => c.LoyaltyTransactions).HasForeignKey(lt => lt.CustomerId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== EMPLOYEES =====
            modelBuilder.Entity<Employee>(e =>
            {
                e.ToTable("employees");
                e.HasIndex(x => x.BranchId).HasDatabaseName("idx_employees_branch_id");
                e.Property(x => x.RoleId).HasComment("Vai trò của nhân viên");
                e.Property(x => x.HourlyRate).HasColumnType("numeric(10,2)").HasComment("Lương theo giờ");
                e.HasOne(em => em.Branch).WithMany().HasForeignKey(em => em.BranchId).OnDelete(DeleteBehavior.Restrict);
            });

            // ===== SHIFTS =====
            modelBuilder.Entity<Shift>(e =>
            {
                e.ToTable("shifts");
                e.HasIndex(x => new { x.BranchId, x.ShiftDate }).HasDatabaseName("idx_shifts_branch_date");
                e.Property(x => x.ActualStart).HasComment("Giờ thực tế nhân viên vào ca (chấm công)");
                e.Property(x => x.ActualEnd).HasComment("Giờ thực tế nhân viên kết thúc ca");
                e.Property(x => x.Status).HasComment("scheduled/active/completed/absent");
                e.HasOne(s => s.Employee).WithMany(em => em.Shifts).HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(s => s.Branch).WithMany().HasForeignKey(s => s.BranchId).OnDelete(DeleteBehavior.Restrict);
            });

            // ===== SUPPLIERS =====
            modelBuilder.Entity<Supplier>(e =>
            {
                e.ToTable("suppliers");
                e.HasIndex(x => x.RestaurantId).HasDatabaseName("idx_suppliers_restaurant_id");
                e.Property(x => x.Name).HasComment("Tên nhà cung cấp");
            });

            // ===== INVENTORY ITEMS =====
            modelBuilder.Entity<InventoryItem>(e =>
            {
                e.ToTable("inventory_items");
                e.HasIndex(x => x.BranchId).HasDatabaseName("idx_inventory_items_branch_id");
                e.Property(x => x.CurrentQuantity).HasColumnType("numeric(12,3)").HasComment("Số lượng tồn kho hiện tại");
                e.Property(x => x.MinQuantity).HasColumnType("numeric(12,3)").HasComment("Mức cảnh báo tồn kho thấp");
                e.Property(x => x.CostPrice).HasColumnType("numeric(12,2)").HasComment("Giá nhập gần nhất");
                e.HasOne(ii => ii.Branch).WithMany().HasForeignKey(ii => ii.BranchId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(ii => ii.Supplier).WithMany(s => s.InventoryItems).HasForeignKey(ii => ii.SupplierId).OnDelete(DeleteBehavior.SetNull);
            });

            // ===== INVENTORY TRANSACTIONS =====
            modelBuilder.Entity<InventoryTransaction>(e =>
            {
                e.ToTable("inventory_transactions");
                e.HasIndex(x => x.InventoryItemId).HasDatabaseName("idx_inventory_transactions_item_id");
                e.Property(x => x.Type).HasComment("import/export/adjust/waste");
                e.Property(x => x.QuantityChange).HasColumnType("numeric(12,3)").HasComment("Số lượng thay đổi (dương = nhập, âm = xuất)");
                e.HasOne(it => it.InventoryItem).WithMany(ii => ii.Transactions).HasForeignKey(it => it.InventoryItemId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== MENU ITEM RECIPES =====
            modelBuilder.Entity<MenuItemRecipe>(e =>
            {
                e.ToTable("menu_item_recipes");
                e.HasIndex(x => x.MenuItemId).HasDatabaseName("idx_recipes_menu_item_id");
                e.Property(x => x.QuantityUsed).HasColumnType("numeric(12,3)").HasComment("Lượng nguyên liệu mỗi suất");
                e.HasOne(r => r.InventoryItem).WithMany(ii => ii.Recipes).HasForeignKey(r => r.InventoryItemId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== PROMOTIONS =====
            modelBuilder.Entity<Promotion>(e =>
            {
                e.ToTable("promotions");
                e.Property(x => x.Type).HasComment("percent_discount/fixed_discount/free_item/buy_x_get_y/loyalty_redeem");
                e.Property(x => x.DiscountValue).HasColumnType("numeric(12,2)").HasComment("Giá trị giảm");
                e.Property(x => x.MinOrderAmount).HasColumnType("numeric(12,2)").HasComment("Giá trị đơn hàng tối thiểu");
                e.Property(x => x.MaxDiscount).HasColumnType("numeric(12,2)").HasComment("Giảm tối đa áp dụng với loại %");
            });

            // ===== VOUCHER CODES =====
            modelBuilder.Entity<VoucherCode>(e =>
            {
                e.ToTable("voucher_codes");
                e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("idx_vouchers_code");
                e.Property(x => x.UsageLimit).HasComment("Số lần dùng tối đa (0 = unlimited)");
                e.HasOne(v => v.Promotion).WithMany(p => p.VoucherCodes).HasForeignKey(v => v.PromotionId).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== NOTIFICATIONS =====
            modelBuilder.Entity<Notification>(e =>
            {
                e.ToTable("notifications");
                e.HasIndex(x => new { x.RecipientId, x.IsRead }).HasDatabaseName("idx_notifications_recipient_read");
                e.Property(x => x.Type).HasComment("order_new/order_ready/inventory_low/reservation_new/payment_completed");
                e.Property(x => x.IsRead).HasComment("Đã đọc chưa");
            });

            // ===== AUDIT LOGS =====
            modelBuilder.Entity<AuditLog>(e =>
            {
                e.ToTable("audit_logs");
                e.HasIndex(x => x.UserId).HasDatabaseName("idx_audit_logs_user_id");
                e.HasIndex(x => x.Module).HasDatabaseName("idx_audit_logs_module");
                e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_audit_logs_created_at");
                e.Property(x => x.OldData).HasColumnType("jsonb").HasComment("Snapshot dữ liệu trước khi thay đổi");
                e.Property(x => x.NewData).HasColumnType("jsonb").HasComment("Snapshot dữ liệu sau khi thay đổi");
            });

            // ===== RESTAURANT ROLES =====
            modelBuilder.Entity<RestaurantRole>(e =>
            {
                e.ToTable("restaurant_roles");
                e.HasIndex(x => x.RestaurantId).HasDatabaseName("idx_restaurant_roles_restaurant_id");
                e.Property(x => x.Permissions).HasColumnType("jsonb").HasComment("Danh sách quyền dạng mảng JSON");
            });
        }

        // Helper for global query filter
        private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression<TInterface>(
            System.Linq.Expressions.Expression<Func<TInterface, bool>> filterExpression,
            Type entityType)
        {
            var newParam = System.Linq.Expressions.Expression.Parameter(entityType);
            var visitor = new TypeReplacingVisitor(filterExpression.Parameters[0], newParam);
            var newBody = visitor.Visit(filterExpression.Body);
            return System.Linq.Expressions.Expression.Lambda(newBody, newParam);
        }

        private class TypeReplacingVisitor : System.Linq.Expressions.ExpressionVisitor
        {
            private readonly System.Linq.Expressions.Expression _source;
            private readonly System.Linq.Expressions.Expression _replacement;

            public TypeReplacingVisitor(System.Linq.Expressions.Expression source, System.Linq.Expressions.Expression replacement)
            {
                _source = source;
                _replacement = replacement;
            }

            protected override System.Linq.Expressions.Expression VisitParameter(System.Linq.Expressions.ParameterExpression node)
                => node == _source ? _replacement : base.VisitParameter(node);
        }
    }
}

