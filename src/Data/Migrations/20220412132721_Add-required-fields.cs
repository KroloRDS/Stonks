using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Addrequiredfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer");

            migrationBuilder.AlterColumn<string>(
                name: "WriterId",
                table: "TradeOffer",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer",
                column: "WriterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer");

            migrationBuilder.AlterColumn<string>(
                name: "WriterId",
                table: "TradeOffer",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeOffer_AspNetUsers_WriterId",
                table: "TradeOffer",
                column: "WriterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
