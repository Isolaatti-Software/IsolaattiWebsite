using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Isolaatti.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToSubColumnInUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Sub",
                table: "Users",
                column: "Sub",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Sub",
                table: "Users");
        }
    }
}
