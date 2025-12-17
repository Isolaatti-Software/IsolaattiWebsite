using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Isolaatti.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteToSquadUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SquadUsers_Squads_SquadId",
                table: "SquadUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_SquadUsers_Squads_SquadId",
                table: "SquadUsers",
                column: "SquadId",
                principalTable: "Squads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SquadUsers_Squads_SquadId",
                table: "SquadUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_SquadUsers_Squads_SquadId",
                table: "SquadUsers",
                column: "SquadId",
                principalTable: "Squads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
