using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Isolaatti.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeysForReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Comments_ResponseForCommentId",
                table: "Comments",
                column: "ResponseForCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ResponseForCommentId",
                table: "Comments",
                column: "ResponseForCommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ResponseForCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ResponseForCommentId",
                table: "Comments");
        }
    }
}
