using Microsoft.EntityFrameworkCore.Migrations;

namespace isolaatti_API.Migrations
{
    public partial class validacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "emailValidated",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "emailValidated",
                table: "Users");
        }
    }
}
