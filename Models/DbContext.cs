/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using Microsoft.EntityFrameworkCore;

namespace isolaatti_API.Models
{
    public class DbContextApp : DbContext
    {
        public DbContextApp(DbContextOptions<DbContextApp> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AdminAccount> AdminAccounts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SimpleTextPost> SimpleTextPosts { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<UserSeenPostHistory> UserSeenPostHistories { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<SessionToken> SessionTokens { get; set; }
        public DbSet<AdminAccountSessionToken> AdminAccountSessionTokens { get; set; }
        public DbSet<PostReport> PostReports { get; set; }
        public DbSet<CommentReport> CommentReports { get; set; }
        public DbSet<GoogleUser> GoogleUsers { get; set; }
        public DbSet<SocialNotification> SocialNotifications { get; set; }
        public DbSet<FollowerRelation> FollowerRelations { get; set; }
        public DbSet<ProfileImage> ProfileImages { get; set; }
        public DbSet<UserProfileLink> UserProfileLinks { get; set; }
        public DbSet<Audio> Audios { get; set; }
    }
}