using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Tempremovereferencecolumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalPrice_Stock_StockId",
                table: "HistoricalPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_StockOwnership_AspNetUsers_OwnerId",
                table: "StockOwnership");

            migrationBuilder.DropForeignKey(
                name: "FK_StockOwnership_Stock_StockId",
                table: "StockOwnership");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AspNetUsers_BuyerId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AspNetUsers_SellerId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Stock_StockId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_BuyerId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SellerId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_StockId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_StockOwnership_OwnerId",
                table: "StockOwnership");

            migrationBuilder.DropIndex(
                name: "IX_StockOwnership_StockId",
                table: "StockOwnership");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalPrice_StockId",
                table: "HistoricalPrice");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "StockOwnership");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "StockOwnership");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "HistoricalPrice");

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "HistoricalPrice",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountTraded",
                table: "HistoricalPrice",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "HistoricalPrice");

            migrationBuilder.DropColumn(
                name: "TotalAmountTraded",
                table: "HistoricalPrice");

            migrationBuilder.AddColumn<string>(
                name: "BuyerId",
                table: "Transaction",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerId",
                table: "Transaction",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "StockOwnership",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "StockOwnership",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Stock",
                type: "decimal(15,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "HistoricalPrice",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_BuyerId",
                table: "Transaction",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SellerId",
                table: "Transaction",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_StockId",
                table: "Transaction",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_StockOwnership_OwnerId",
                table: "StockOwnership",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StockOwnership_StockId",
                table: "StockOwnership",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalPrice_StockId",
                table: "HistoricalPrice",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalPrice_Stock_StockId",
                table: "HistoricalPrice",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockOwnership_AspNetUsers_OwnerId",
                table: "StockOwnership",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockOwnership_Stock_StockId",
                table: "StockOwnership",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AspNetUsers_BuyerId",
                table: "Transaction",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AspNetUsers_SellerId",
                table: "Transaction",
                column: "SellerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Stock_StockId",
                table: "Transaction",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
