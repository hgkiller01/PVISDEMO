using Microsoft.EntityFrameworkCore.Migrations;

namespace Pvis.Web.Data.Migrations
{
    public partial class ver006 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppPid",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppPid",
                table: "AspNetUsers");
        }
    }
}
