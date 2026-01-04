using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Isolaatti.Migrations
{
    /// <inheritdoc />
    public partial class UsersTableToUseSub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "Sub");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "LastName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sub",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Users",
                newName: "Email");
        }
    }
}
