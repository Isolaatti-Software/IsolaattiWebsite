/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using Microsoft.EntityFrameworkCore;
using isolaatti_API.Models;
namespace isolaatti_API.Models
{
    public class DbContextApp : DbContext
    {
        public DbContextApp (DbContextOptions<DbContextApp> options) : base(options)
        {
            
        }
        
        public DbSet<Song> Songs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ProcessingServerList> Servers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SongShares> SharedSongs { get; set; }
        public DbSet<UserUsageData> UsageData { get; set; }
        public DbSet<AdminAccount> AdminAccounts { get; set; }
        public DbSet<SongQueue> SongsQueue { get; set; }
        public DbSet<CustomTrack> CustomTracks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SimpleTextPost> SimpleTextPosts { get; set; }
        public DbSet<ProjectComment> ProjectComments { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<UserSeenPostHistory> UserSeenPostHistories { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<SessionToken> SessionTokens { get; set; }
        public DbSet<AdminAccountSessionToken> AdminAccountSessionTokens { get; set; }
    }
}