using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "custom_roles");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.CreateTable(
                name: "restaurant_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<string>(type: "jsonb", nullable: false, comment: "Danh sách quyền dạng mảng JSON"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurant_roles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_restaurant_roles_restaurant_id",
                table: "restaurant_roles",
                column: "RestaurantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "restaurant_roles");

            migrationBuilder.CreateTable(
                name: "custom_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false, comment: "Mã vai trò (dùng liên kết)"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false, comment: "Tên hiển thị của vai trò"),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false, comment: "Quyền truy cập: True = Có, False = Không"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Module = table.Column<string>(type: "text", nullable: false, comment: "Tên module (VD: /dashboard)"),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false, comment: "Vai trò bị giới hạn"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_custom_roles_unique",
                table: "custom_roles",
                columns: new[] { "RestaurantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_unique",
                table: "role_permissions",
                columns: new[] { "RestaurantId", "Role", "Module" },
                unique: true);
        }
    }
}
