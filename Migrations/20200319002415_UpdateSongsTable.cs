﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace isolaatti_API.Migrations
{
    public partial class UpdateSongsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isBeingProcessed",
                table: "Songs",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isBeingProcessed",
                table: "Songs");
        }
    }
}
