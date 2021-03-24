using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace isolaatti_API.Migrations
{
    public partial class localDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(nullable: true),
                    password = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextContent = table.Column<string>(nullable: true),
                    WhoWrote = table.Column<int>(nullable: false),
                    SimpleTextPostId = table.Column<int>(nullable: false),
                    Privacy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomTracks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SongId = table.Column<int>(nullable: false),
                    DownloadUrl = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    EffectsAndPropertiesDefinitionJsonObject = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomTracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    SongId = table.Column<int>(nullable: false),
                    SongName = table.Column<string>(nullable: true),
                    ArtistName = table.Column<string>(nullable: true),
                    Seen = table.Column<bool>(nullable: false),
                    type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectComments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextContent = table.Column<string>(nullable: true),
                    WhoWroteId = table.Column<int>(nullable: false),
                    ProjectInt = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(nullable: true),
                    OnQueue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedSongs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SharedSongId = table.Column<int>(nullable: false),
                    uid = table.Column<string>(nullable: true),
                    userId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedSongs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimpleTextPosts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextContent = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    NumberOfLikes = table.Column<long>(nullable: false),
                    Privacy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleTextPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(nullable: false),
                    OriginalFileName = table.Column<string>(nullable: true),
                    Artist = table.Column<string>(nullable: true),
                    BassUrl = table.Column<string>(nullable: true),
                    DrumsUrl = table.Column<string>(nullable: true),
                    VoiceUrl = table.Column<string>(nullable: true),
                    OtherUrl = table.Column<string>(nullable: true),
                    EffectsDefinitionJsonArray = table.Column<string>(nullable: true),
                    TracksSettings = table.Column<string>(nullable: true),
                    IsBeingProcessed = table.Column<bool>(nullable: false),
                    Uid = table.Column<string>(nullable: true),
                    NumberOfLikes = table.Column<long>(nullable: false),
                    IsPublicInApp = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SongsQueue",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudioSourceUrl = table.Column<string>(nullable: true),
                    Reserved = table.Column<bool>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    SongName = table.Column<string>(nullable: true),
                    SongArtist = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongsQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    SoloOnDrums = table.Column<long>(nullable: false),
                    SoloOnBass = table.Column<long>(nullable: false),
                    SoloOnVocals = table.Column<long>(nullable: false),
                    SoloOnOther = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    Uid = table.Column<string>(nullable: true),
                    EmailValidated = table.Column<bool>(nullable: false),
                    GoogleToken = table.Column<string>(nullable: true),
                    NotifyByEmail = table.Column<bool>(nullable: false),
                    NotifyWhenProcessStarted = table.Column<bool>(nullable: false),
                    NotifyWhenProcessFinishes = table.Column<bool>(nullable: false),
                    AppLanguage = table.Column<string>(nullable: true),
                    FollowersIdsJson = table.Column<string>(nullable: true),
                    FollowingIdsJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSeenPostHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    PostId = table.Column<long>(nullable: false),
                    TimesSeen = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSeenPostHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    Expires = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAccounts");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CustomTracks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProjectComments");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "SharedSongs");

            migrationBuilder.DropTable(
                name: "SimpleTextPosts");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "SongsQueue");

            migrationBuilder.DropTable(
                name: "UsageData");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserSeenPostHistories");

            migrationBuilder.DropTable(
                name: "UserTokens");
        }
    }
}
