using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Models
{
    public class DbContextApp : DbContext, IDataProtectionKeyContext
    {
        public DbContextApp(DbContextOptions<DbContextApp> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UniqueUsername)
                .HasFilter("'UniqueUsername' IS NOT NULL")
                .IsUnique();
        }


        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        
        public DbSet<User> Users { get; set; }
        public DbSet<ChangePasswordToken> ChangePasswordTokens { get; set; }
        public DbSet<ExternalUser> ExternalUsers { get; set; }
        public DbSet<UserSeenPostHistory> UserSeenPostHistories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> SimpleTextPosts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<FollowerRelation> FollowerRelations { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<SquadUser> SquadUsers { get; set; }
        public DbSet<TrackingUserInteraction> TrackingUserInteractions { get; set; }
    }
}