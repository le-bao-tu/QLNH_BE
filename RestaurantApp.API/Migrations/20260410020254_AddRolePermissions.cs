using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Module = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldData = table.Column<string>(type: "jsonb", nullable: true, comment: "Snapshot dữ liệu trước khi thay đổi"),
                    NewData = table.Column<string>(type: "jsonb", nullable: true, comment: "Snapshot dữ liệu sau khi thay đổi"),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    LoyaltyPoints = table.Column<int>(type: "integer", nullable: false, comment: "Điểm tích lũy hiện tại"),
                    LoyaltyTier = table.Column<string>(type: "text", nullable: false, comment: "bronze/silver/gold/platinum"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menu_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "Thứ tự hiển thị danh mục trên menu"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "Danh mục đang được hiển thị hay ẩn"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false, comment: "order_new/order_ready/inventory_low/reservation_new/payment_completed"),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, comment: "Đã đọc chưa"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false, comment: "percent_discount/fixed_discount/free_item/buy_x_get_y/loyalty_redeem"),
                    DiscountValue = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Giá trị giảm"),
                    ApplyTo = table.Column<string>(type: "text", nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Giá trị đơn hàng tối thiểu"),
                    MaxDiscount = table.Column<decimal>(type: "numeric(12,2)", nullable: true, comment: "Giảm tối đa áp dụng với loại %"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MenuItemIds = table.Column<string>(type: "text", nullable: true),
                    BranchIds = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false, comment: "Tên nhà hàng"),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID chủ nhà hàng"),
                    LogoUrl = table.Column<string>(type: "text", nullable: true, comment: "Đường dẫn logo nhà hàng"),
                    TaxCode = table.Column<string>(type: "text", nullable: true, comment: "Mã số thuế doanh nghiệp"),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    BankId = table.Column<string>(type: "text", nullable: true),
                    BankNumber = table.Column<string>(type: "text", nullable: true),
                    BankOwner = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false, comment: "Vai trò bị giới hạn"),
                    Module = table.Column<string>(type: "text", nullable: false, comment: "Tên module (VD: /dashboard)"),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false, comment: "Quyền truy cập: True = Có, False = Không"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false, comment: "Tên nhà cung cấp"),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false, comment: "Tên đăng nhập"),
                    Email = table.Column<string>(type: "text", nullable: false, comment: "Email đăng nhập"),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false, comment: "Vai trò: Admin/Manager/Staff"),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PointsChange = table.Column<int>(type: "integer", nullable: false, comment: "Số điểm thay đổi (dương = cộng, âm = trừ)"),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BalanceAfter = table.Column<int>(type: "integer", nullable: false, comment: "Điểm còn lại sau giao dịch"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loyalty_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loyalty_transactions_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchIds = table.Column<string>(type: "text", nullable: true, comment: "JSON array of branch ids. NULL = áp dụng cho tất cả chi nhánh"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Giá mặc định của món"),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, comment: "FALSE khi món tạm hết hoặc không phục vụ"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_items_menu_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "menu_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "voucher_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: false, comment: "Số lần dùng tối đa (0 = unlimited)"),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
                    AssignedTo = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_voucher_codes_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false, comment: "Tên chi nhánh"),
                    Address = table.Column<string>(type: "text", nullable: false, comment: "Địa chỉ chi nhánh"),
                    Phone = table.Column<string>(type: "text", nullable: false, comment: "Số điện thoại chi nhánh"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_branches_restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_combos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComboItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SingleItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_combos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_item_combos_menu_items_ComboItemId",
                        column: x => x.ComboItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_combos_menu_items_SingleItemId",
                        column: x => x.SingleItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_option_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "text", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, comment: "TRUE = khách bắt buộc phải chọn"),
                    IsMultiple = table.Column<bool>(type: "boolean", nullable: false, comment: "TRUE = khách được chọn nhiều option cùng lúc"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_option_groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_item_option_groups_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false, comment: "owner/manager/cashier/waiter/chef/bartender"),
                    HourlyRate = table.Column<decimal>(type: "numeric(10,2)", nullable: false, comment: "Lương theo giờ"),
                    HiredAt = table.Column<DateOnly>(type: "date", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employees_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    CurrentQuantity = table.Column<decimal>(type: "numeric(12,3)", nullable: false, comment: "Số lượng tồn kho hiện tại"),
                    MinQuantity = table.Column<decimal>(type: "numeric(12,3)", nullable: false, comment: "Mức cảnh báo tồn kho thấp"),
                    CostPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Giá nhập gần nhất"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_items_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_items_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TableNumber = table.Column<int>(type: "integer", nullable: false, comment: "Số bàn hiển thị"),
                    Capacity = table.Column<int>(type: "integer", nullable: false, comment: "Sức chứa tối đa"),
                    Status = table.Column<string>(type: "text", nullable: false, comment: "Trạng thái: available/occupied/reserved/cleaning"),
                    Floor = table.Column<int>(type: "integer", nullable: true, comment: "Tầng hoặc khu vực đặt bàn"),
                    QrCode = table.Column<string>(type: "text", nullable: true, comment: "Mã QR để khách tự order"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tables_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Giá cộng thêm so với giá gốc của món"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_item_options_menu_item_option_groups_OptionGroupId",
                        column: x => x.OptionGroupId,
                        principalTable: "menu_item_option_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ActualStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Giờ thực tế nhân viên vào ca (chấm công)"),
                    ActualEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Giờ thực tế nhân viên kết thúc ca"),
                    Status = table.Column<string>(type: "text", nullable: false, comment: "scheduled/active/completed/absent"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shifts_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shifts_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false, comment: "import/export/adjust/waste"),
                    QuantityChange = table.Column<decimal>(type: "numeric(12,3)", nullable: false, comment: "Số lượng thay đổi (dương = nhập, âm = xuất)"),
                    QuantityAfter = table.Column<decimal>(type: "numeric", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_inventory_items_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "inventory_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityUsed = table.Column<decimal>(type: "numeric(12,3)", nullable: false, comment: "Lượng nguyên liệu mỗi suất"),
                    Unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_item_recipes_inventory_items_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "inventory_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_recipes_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TableId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, comment: "pending/preparing/ready/served/paid/cancelled"),
                    Subtotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Tổng tiền trước giảm giá"),
                    DiscountAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Số tiền được giảm"),
                    TaxAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Thuế VAT"),
                    TotalAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Tổng tiền phải trả"),
                    VoucherId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuestCount = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orders_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_orders_tables_TableId",
                        column: x => x.TableId,
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TableId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuestName = table.Column<string>(type: "text", nullable: true),
                    GuestPhone = table.Column<string>(type: "text", nullable: true),
                    GuestEmail = table.Column<string>(type: "text", nullable: true),
                    PartySize = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng khách dự kiến"),
                    ReservedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Thời điểm khách muốn đến"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false, comment: "pending/confirmed/seated/completed/cancelled/no_show"),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservations_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reservations_tables_TableId",
                        column: x => x.TableId,
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Giá tại thời điểm order, không thay đổi dù menu cập nhật sau"),
                    TotalPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Thành tiền"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false, comment: "pending/cooking/ready/served/cancelled"),
                    SentToKitchenAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Thời điểm nhân viên gửi món vào bếp"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_items_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_order_items_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Số tiền thanh toán lần này"),
                    Method = table.Column<string>(type: "text", nullable: false, comment: "Phương thức: cash/momo/zalopay/vnpay/credit_card..."),
                    ReferenceCode = table.Column<string>(type: "text", nullable: true, comment: "Mã giao dịch từ cổng thanh toán bên ngoài"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "table_transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferredBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Thời điểm chuyển bàn"),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_table_transfers_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kitchen_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TableNumber = table.Column<int>(type: "integer", nullable: false, comment: "Snapshot số bàn để hiển thị nhanh trên KDS"),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, comment: "Số càng cao càng ưu tiên chế biến trước"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Thời điểm ticket xuất hiện ở bếp"),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kitchen_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kitchen_orders_order_items_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "order_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionName = table.Column<string>(type: "text", nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false, comment: "Giá thêm snapshot tại thời điểm order")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_item_options_order_items_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "order_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_created_at",
                table: "audit_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_module",
                table: "audit_logs",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_user_id",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_branches_restaurant_id",
                table: "branches",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "idx_customers_restaurant_phone",
                table: "customers",
                columns: new[] { "RestaurantId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "idx_employees_branch_id",
                table: "employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "idx_inventory_items_branch_id",
                table: "inventory_items",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_SupplierId",
                table: "inventory_items",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "idx_inventory_transactions_item_id",
                table: "inventory_transactions",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "idx_kitchen_orders_branch_status",
                table: "kitchen_orders",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_orders_OrderItemId",
                table: "kitchen_orders",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_transactions_customer_id",
                table: "loyalty_transactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "idx_menu_categories_restaurant_id",
                table: "menu_categories",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "idx_menu_item_combos_combo_id",
                table: "menu_item_combos",
                column: "ComboItemId");

            migrationBuilder.CreateIndex(
                name: "idx_menu_item_combos_single_id",
                table: "menu_item_combos",
                column: "SingleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_option_groups_MenuItemId",
                table: "menu_item_option_groups",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_options_OptionGroupId",
                table: "menu_item_options",
                column: "OptionGroupId");

            migrationBuilder.CreateIndex(
                name: "idx_recipes_menu_item_id",
                table: "menu_item_recipes",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_recipes_InventoryItemId",
                table: "menu_item_recipes",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "idx_menu_items_category_id",
                table: "menu_items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_recipient_read",
                table: "notifications",
                columns: new[] { "RecipientId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_order_item_options_OrderItemId",
                table: "order_item_options",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "idx_order_items_order_id",
                table: "order_items",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_MenuItemId",
                table: "order_items",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "idx_orders_branch_id",
                table: "orders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "idx_orders_created_at",
                table: "orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_orders_status",
                table: "orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_orders_table_id",
                table: "orders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_CustomerId",
                table: "orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "idx_payments_order_id",
                table: "payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "idx_reservations_branch_id",
                table: "reservations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "idx_reservations_reserved_at",
                table: "reservations",
                column: "ReservedAt");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_CustomerId",
                table: "reservations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_TableId",
                table: "reservations",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_unique",
                table: "role_permissions",
                columns: new[] { "RestaurantId", "Role", "Module" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_shifts_branch_date",
                table: "shifts",
                columns: new[] { "BranchId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_shifts_EmployeeId",
                table: "shifts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "idx_suppliers_restaurant_id",
                table: "suppliers",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_table_transfers_OrderId",
                table: "table_transfers",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "idx_tables_branch_id",
                table: "tables",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "idx_tables_branch_number",
                table: "tables",
                columns: new[] { "BranchId", "TableNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_username",
                table: "users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_vouchers_code",
                table: "voucher_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voucher_codes_PromotionId",
                table: "voucher_codes",
                column: "PromotionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "inventory_transactions");

            migrationBuilder.DropTable(
                name: "kitchen_orders");

            migrationBuilder.DropTable(
                name: "loyalty_transactions");

            migrationBuilder.DropTable(
                name: "menu_item_combos");

            migrationBuilder.DropTable(
                name: "menu_item_options");

            migrationBuilder.DropTable(
                name: "menu_item_recipes");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "order_item_options");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "shifts");

            migrationBuilder.DropTable(
                name: "table_transfers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "voucher_codes");

            migrationBuilder.DropTable(
                name: "menu_item_option_groups");

            migrationBuilder.DropTable(
                name: "inventory_items");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "promotions");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "menu_categories");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "tables");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "restaurants");
        }
    }
}
