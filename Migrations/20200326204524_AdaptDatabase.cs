using Microsoft.EntityFrameworkCore.Migrations;

namespace isolaatti_API.Migrations
{
    public partial class AdaptDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isBeingProcessed",
                table: "Songs",
                newName: "IsBeingProcessed");

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "Songs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Artist",
                table: "Songs");

            migrationBuilder.RenameColumn(
                name: "IsBeingProcessed",
                table: "Songs",
                newName: "isBeingProcessed");
        }
    }
}
