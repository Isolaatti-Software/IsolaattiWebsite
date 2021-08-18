using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace isolaatti_API.Migrations
{
    public partial class AddedNewFieldsIntoUserAndPost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionAudioUrl",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionText",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFollowers",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFollowing",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "SimpleTextPosts",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Comments",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAudioUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DescriptionText",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NumberOfFollowers",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NumberOfFollowing",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "SimpleTextPosts");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Comments");
        }
    }
}
