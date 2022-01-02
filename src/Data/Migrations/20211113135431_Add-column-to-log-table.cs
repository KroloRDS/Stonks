using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stonks.Data.Migrations
{
    public partial class Addcolumntologtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Details",
                table: "Log",
                newName: "Exception");

            migrationBuilder.AddColumn<string>(
                name: "ObjectDump",
                table: "Log",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectDump",
                table: "Log");

            migrationBuilder.RenameColumn(
                name: "Exception",
                table: "Log",
                newName: "Details");
        }
    }
}
