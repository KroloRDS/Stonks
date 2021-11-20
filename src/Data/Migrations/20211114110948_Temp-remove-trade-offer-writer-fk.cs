using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Tempremovetradeofferwriterfk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer");

            migrationBuilder.DropIndex(
                name: "IX_TradeOffer_WriterId",
                table: "TradeOffer");

            migrationBuilder.DropColumn(
                name: "WriterId",
                table: "TradeOffer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WriterId",
                table: "TradeOffer",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_WriterId",
                table: "TradeOffer",
                column: "WriterId");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer",
                column: "WriterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
