using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Accounts.SignIn.Data;
using Isolaatti.Audios.Data;
using Isolaatti.Comments.Entity;
using Isolaatti.Favorites.Data;
using Isolaatti.Images.Data;
using Isolaatti.Likes.Data;
using Isolaatti.Models;
using Isolaatti.Notifications.Entity;
using Isolaatti.Posting.Data;
using Isolaatti.Report.Entity;
using Isolaatti.Search.Data;
using Isolaatti.Squads.Data;
using Isolaatti.Tagging.Entity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Database.EF
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
            
            modelBuilder.Entity<FollowerRelation>()
                .HasOne<User>("User")
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FollowerRelation>()
                .HasOne<User>("TargetUser")
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SearchResultViewEntity>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("View_SearchItems");
                eb.Property(v => v.Origin).HasColumnName("origin_table");
                eb.Property(v => v.ResourceId).HasColumnName("id");
                eb.Property(v => v.Title).HasColumnName("title");
                eb.Property(v => v.Description).HasColumnName("description");
                eb.Property(v => v.Rank).HasColumnName("rank");
            });

            modelBuilder.Entity<Post>()
                .HasMany<Image>(d => d.Images)
                .WithOne(image => image.Post)
                .HasForeignKey(image => image.PostId)
                .HasPrincipalKey(post => post.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .HasOne<Audio>(p => p.Audio)
                .WithOne(audio => audio.Post)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Audio>()
                .HasOne<User>()
                .WithOne(u => u.DescriptionAudio)
                .HasForeignKey<User>(user => user.DescriptionAudioId)
                .OnDelete(DeleteBehavior.SetNull);
        }


        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        
        public DbSet<User> Users { get; set; }
        public DbSet<AccountPrecreate> AccountPrecreates { get; set; }
        public DbSet<ChangePasswordToken> ChangePasswordTokens { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> SimpleTextPosts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<FollowerRelation> FollowerRelations { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<SquadUser> SquadUsers { get; set; }
        public DbSet<TrackingUserInteraction> TrackingUserInteractions { get; set; }
        public DbSet<FavoriteEntity> Favorites { get; set; }
        public DbSet<HashtagEntity> Hashtags { get; set; }
        public DbSet<UserTagEntity> UserTags { get; set; }
        public DbSet<AccountRemovalRequestEntity> AccountRemovalRequests { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }
        public DbSet<SearchResultViewEntity> SearchView { get; set; }
        public DbSet<ReportEntity> Reports { get; set; }
        public DbSet<PostVersionEntity> PostVersions { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Audio> Audios { get; set; }
    }
}