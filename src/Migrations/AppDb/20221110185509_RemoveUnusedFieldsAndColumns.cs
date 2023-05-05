using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Migrations.AppDb
{
    public partial class RemoveUnusedFieldsAndColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvgPriceCurrent");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "AvgPrice");

            migrationBuilder.DropColumn(
                name: "PriceNormalised",
                table: "AvgPrice");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPriceUpdate",
                table: "Stock",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Stock",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SharesTraded",
                table: "Stock",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPriceUpdate",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "SharesTraded",
                table: "Stock");

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "AvgPrice",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceNormalised",
                table: "AvgPrice",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AvgPriceCurrent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    SharesTraded = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvgPriceCurrent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvgPriceCurrent_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvgPriceCurrent_StockId",
                table: "AvgPriceCurrent",
                column: "StockId");
        }
    }
}
