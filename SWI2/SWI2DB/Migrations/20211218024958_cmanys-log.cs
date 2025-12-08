using Microsoft.EntityFrameworkCore.Migrations;

namespace SWI2DB.Migrations
{
    public partial class cmanyslog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Companys",
                table: "Log",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Companys",
                table: "Log");
        }
    }
}
