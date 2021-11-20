using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Renamecolumnsintradeoffertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinPrice",
                table: "TradeOffer",
                newName: "SellPrice");

            migrationBuilder.RenameColumn(
                name: "MaxPrice",
                table: "TradeOffer",
                newName: "BuyPrice");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellPrice",
                table: "TradeOffer",
                newName: "MinPrice");

            migrationBuilder.RenameColumn(
                name: "BuyPrice",
                table: "TradeOffer",
                newName: "MaxPrice");
        }
    }
}
