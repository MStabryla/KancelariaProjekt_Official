using Microsoft.EntityFrameworkCore.Migrations;

namespace SWI2DB.Migrations
{
    public partial class logfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Log",
                newName: "Logged");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Logged",
                table: "Log",
                newName: "Type");
        }
    }
}
