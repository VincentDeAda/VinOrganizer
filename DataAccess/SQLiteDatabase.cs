
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class SQLiteDatabase : DbContext
{
    public DbSet<Extension> Extensions { get; set; } = null!;
    public DbSet<ExtensionPack> ExtensionPacks { get; set; } = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        string connectionStringDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VDA", "VinOrg");
        string connectionString = Path.Combine(connectionStringDir, "extensions.db");

        Directory.CreateDirectory(connectionStringDir);
        optionsBuilder.UseSqlite($"Data Source={connectionString}");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExtensionPack>(x =>
        {
            x.Property(x => x.Id).ValueGeneratedOnAdd();
            x.HasIndex(x => x.Name).IsUnique();
        });
        modelBuilder.Entity<Extension>(x =>
        {
            x.HasOne(x => x.ExtensionPack).WithMany(x => x.Extensions);
            x.Property(x => x.Id).ValueGeneratedOnAdd();
        });
    }
}
