using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Adduserfunds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Funds",
                table: "AspNetUsers",
                type: "decimal(11,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Funds",
                table: "AspNetUsers");
        }
    }
}
