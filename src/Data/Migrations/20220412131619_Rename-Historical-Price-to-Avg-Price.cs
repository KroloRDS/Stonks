using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class RenameHistoricalPricetoAvgPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricalPrice");

            migrationBuilder.CreateTable(
                name: "AvgPrice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    TotalAmountTraded = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    AmountNormalised = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvgPrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvgPrice_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvgPrice_StockId",
                table: "AvgPrice",
                column: "StockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvgPrice");

            migrationBuilder.CreateTable(
                name: "HistoricalPrice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AveragePrice = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    PriceNormalised = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    TotalAmountTraded = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalPrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalPrice_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalPrice_StockId",
                table: "HistoricalPrice",
                column: "StockId");
        }
    }
}
