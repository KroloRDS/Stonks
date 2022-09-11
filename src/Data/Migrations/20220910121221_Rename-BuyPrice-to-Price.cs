using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class RenameBuyPricetoPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BuyPrice",
                table: "TradeOffer",
                newName: "Price");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "TradeOffer",
                newName: "BuyPrice");
        }
    }
}
