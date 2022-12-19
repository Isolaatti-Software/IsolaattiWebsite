using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Models;

public class MyKeysDbContext : DbContext, IDataProtectionKeyContext
{
    public MyKeysDbContext(DbContextOptions<MyKeysDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}