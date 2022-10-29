﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class tempremovestringidcolumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Share_AspNetUsers_OwnerId",
                table: "Share");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AspNetUsers_BuyerId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AspNetUsers_SellerId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_BuyerId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SellerId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_TradeOffer_WriterId",
                table: "TradeOffer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Share",
                table: "Share");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "WriterId",
                table: "TradeOffer");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Share");

            migrationBuilder.DropColumn(
                name: "PayPalEmail",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerId",
                table: "Transaction",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerId",
                table: "Transaction",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WriterId",
                table: "TradeOffer",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Share",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PayPalEmail",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Share",
                table: "Share",
                columns: new[] { "OwnerId", "StockId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_BuyerId",
                table: "Transaction",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SellerId",
                table: "Transaction",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_WriterId",
                table: "TradeOffer",
                column: "WriterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Share_AspNetUsers_OwnerId",
                table: "Share",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer",
                column: "WriterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AspNetUsers_BuyerId",
                table: "Transaction",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AspNetUsers_SellerId",
                table: "Transaction",
                column: "SellerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
