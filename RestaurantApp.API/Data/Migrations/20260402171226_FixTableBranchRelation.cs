using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTableBranchRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tables_branches_BranchId1",
                table: "tables");

            migrationBuilder.DropIndex(
                name: "IX_tables_BranchId1",
                table: "tables");

            migrationBuilder.DropColumn(
                name: "BranchId1",
                table: "tables");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId1",
                table: "tables",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tables_BranchId1",
                table: "tables",
                column: "BranchId1");

            migrationBuilder.AddForeignKey(
                name: "FK_tables_branches_BranchId1",
                table: "tables",
                column: "BranchId1",
                principalTable: "branches",
                principalColumn: "Id");
        }
    }
}
