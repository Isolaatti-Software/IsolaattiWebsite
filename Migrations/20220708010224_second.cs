using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Isolaatti.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DescriptionAudioUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FollowersIdsJson",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FollowingIdsJson",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GoogleToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Uid",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "DescriptionAudioId",
                table: "Users",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAudioId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AppLanguage",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAudioUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowersIdsJson",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowingIdsJson",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
