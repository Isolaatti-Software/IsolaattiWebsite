using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Isolaatti.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColumnsInComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LinkedDiscussionId",
                table: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LinkedCommentId",
                table: "Comments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LinkedDiscussionId",
                table: "Comments",
                type: "bigint",
                nullable: true);
        }
    }
}
