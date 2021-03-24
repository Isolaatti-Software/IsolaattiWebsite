using Microsoft.EntityFrameworkCore.Migrations;

namespace isolaatti_API.Migrations
{
    public partial class AddedTargetUserIdColumnToLikesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetUserId",
                table: "Likes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Likes");
        }
    }
}
