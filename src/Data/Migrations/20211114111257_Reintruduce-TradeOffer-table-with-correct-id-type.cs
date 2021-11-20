using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class ReintruduceTradeOffertablewithcorrectidtype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradeOffer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WriterId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    MaxPrice = table.Column<decimal>(type: "decimal(15,9)", nullable: false),
                    MinPrice = table.Column<decimal>(type: "decimal(15,9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeOffer_AspNetUsers_WriterId",
                        column: x => x.WriterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TradeOffer_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_StockId",
                table: "TradeOffer",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_WriterId",
                table: "TradeOffer",
                column: "WriterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradeOffer");
        }
    }
}
