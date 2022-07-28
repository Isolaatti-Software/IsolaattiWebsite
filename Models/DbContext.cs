/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Models
{
    public class DbContextApp : DbContext
    {
        public DbContextApp(DbContextOptions<DbContextApp> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ChangePasswordToken> ChangePasswordTokens { get; set; }
        public DbSet<SessionToken> SessionTokens { get; set; }
        public DbSet<ExternalUser> ExternalUsers { get; set; }
        // public DbSet<SocialNotification> SocialNotifications { get; set; }
        public DbSet<UserProfileLink> UserProfileLinks { get; set; }

        public DbSet<UserSeenPostHistory> UserSeenPostHistories { get; set; }

        // This model will be dropped, a new webapp will be made
        // public DbSet<AdminAccount> AdminAccounts { get; set; }
        // public DbSet<AdminAccountSessionToken> AdminAccountSessionTokens { get; set; }

        // These models will be dropped from this app and moved to the service "Discussions"
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SimpleTextPost> SimpleTextPosts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<FollowerRelation> FollowerRelations { get; set; }

        // These models will be dropped from this app and moved to the service "Images"
        public DbSet<ProfileImage> ProfileImages { get; set; }


        // These models will be dropped from this app and moved to the service "User reports"
        // public DbSet<PostReport> PostReports { get; set; }
        // public DbSet<CommentReport> CommentReports { get; set; }
        
        public DbSet<Squad> Squads { get; set; }
        public DbSet<SquadUser> SquadUsers { get; set; }
    }
}