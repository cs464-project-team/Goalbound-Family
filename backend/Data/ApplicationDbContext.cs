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
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }

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

        // Configure Receipt entity
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.UploadedAt);

            entity.Property(r => r.UploadedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ReceiptItem entity
        modelBuilder.Entity<ReceiptItem>(entity =>
        {
            entity.HasIndex(ri => ri.ReceiptId);
            entity.HasIndex(ri => ri.LineNumber);

            entity.Property(ri => ri.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(ri => ri.Receipt)
                .WithMany(r => r.Items)
                .HasForeignKey(ri => ri.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Add more entity configurations as needed
    }
}
