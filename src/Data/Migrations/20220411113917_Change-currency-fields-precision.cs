using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Changecurrencyfieldsprecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Transaction",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SellPrice",
                table: "TradeOffer",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BuyPrice",
                table: "TradeOffer",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceNormalised",
                table: "HistoricalPrice",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AveragePrice",
                table: "HistoricalPrice",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,9)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Transaction",
                type: "decimal(15,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SellPrice",
                table: "TradeOffer",
                type: "decimal(15,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BuyPrice",
                table: "TradeOffer",
                type: "decimal(15,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceNormalised",
                table: "HistoricalPrice",
                type: "decimal(15,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AveragePrice",
                table: "HistoricalPrice",
                type: "decimal(15,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
