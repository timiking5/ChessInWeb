global using Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DataAccess;

/// <summary>
/// DbContext class that inherits from IdentityDbContext
/// to provide logging functionality
/// </summary>
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Game> Games { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Game>()
            .HasOne(x => x.WhitePlayer)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Game>()
            .HasOne(x => x.BlackPlayer)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Game>()
            .HasOne(x => x.Winner)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Game>().Property(m => m.WinnerId).IsRequired(false);
        modelBuilder.Entity<Game>().Property(m => m.WhitePlayerId).IsRequired(false);
        modelBuilder.Entity<Game>().Property(m => m.BlackPlayerId).IsRequired(false);
    }
}
