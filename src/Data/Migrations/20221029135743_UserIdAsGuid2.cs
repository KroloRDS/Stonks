using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class UserIdAsGuid2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Share_OwnerId",
                table: "Share");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Share",
                table: "Share",
                columns: new[] { "OwnerId", "StockId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Share",
                table: "Share");

            migrationBuilder.CreateIndex(
                name: "IX_Share_OwnerId",
                table: "Share",
                column: "OwnerId");
        }
    }
}
