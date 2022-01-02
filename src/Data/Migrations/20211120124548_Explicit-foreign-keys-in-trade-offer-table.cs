using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Explicitforeignkeysintradeoffertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "TradeOffer",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "WriterId",
                table: "TradeOffer",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_StockId",
                table: "TradeOffer",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_WriterId",
                table: "TradeOffer",
                column: "WriterId");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer",
                column: "WriterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffer_Stock_StockId",
                table: "TradeOffer",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffer_Stock_StockId",
                table: "TradeOffer");

            migrationBuilder.DropIndex(
                name: "IX_TradeOffer_StockId",
                table: "TradeOffer");

            migrationBuilder.DropIndex(
                name: "IX_TradeOffer_WriterId",
                table: "TradeOffer");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "TradeOffer");

            migrationBuilder.DropColumn(
                name: "WriterId",
                table: "TradeOffer");
        }
    }
}
