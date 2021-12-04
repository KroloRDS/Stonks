using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Transactiontimestampandhistoricalpricecolumnrename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "HistoricalPrice",
                newName: "AveragePrice");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "Transaction",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Timestamp",
                table: "Transaction",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_Timestamp",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "AveragePrice",
                table: "HistoricalPrice",
                newName: "Price");
        }
    }
}
