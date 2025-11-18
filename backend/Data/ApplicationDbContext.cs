using GoalboundFamily.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for entities
    public DbSet<User> Users { get; set; }

    // Add more DbSets as needed
    // Example: public DbSet<Family> Families { get; set; }
    // Example: public DbSet<Goal> Goals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("NOW()");
        });

        // Add more entity configurations as needed
    }
}
