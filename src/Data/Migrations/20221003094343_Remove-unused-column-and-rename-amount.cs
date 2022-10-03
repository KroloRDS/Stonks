using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Removeunusedcolumnandrenameamount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellPrice",
                table: "TradeOffer");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "AvgPriceCurrent",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "AmountNormalised",
                table: "AvgPrice",
                newName: "PriceNormalised");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "AvgPrice",
                newName: "Price");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "AvgPriceCurrent",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "PriceNormalised",
                table: "AvgPrice",
                newName: "AmountNormalised");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "AvgPrice",
                newName: "Amount");

            migrationBuilder.AddColumn<decimal>(
                name: "SellPrice",
                table: "TradeOffer",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
