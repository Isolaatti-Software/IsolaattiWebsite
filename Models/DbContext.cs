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
    }
}