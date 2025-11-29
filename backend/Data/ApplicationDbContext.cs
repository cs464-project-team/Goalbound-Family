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
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<BudgetCategory> BudgetCategories { get; set; }
    public DbSet<HouseholdBudget> HouseholdBudgets { get; set; }
    public DbSet<Expense> Expenses { get; set; }

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

        // Configure Household entity
        modelBuilder.Entity<Household>(entity =>
        {
            entity.Property(h => h.CreatedAt)
                .HasDefaultValueSql("NOW()");
        });

        // Configure HouseholdMember entity
        modelBuilder.Entity<HouseholdMember>(entity =>
        {
            entity.HasIndex(hm => new { hm.HouseholdId, hm.UserId })
                .IsUnique();

            // User relationship
            entity.HasOne(hm => hm.User)
                .WithMany(u => u.HouseholdMemberships)
                .HasForeignKey(hm => hm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Household relationship
            entity.HasOne(hm => hm.Household)
                .WithMany(h => h.Members)
                .HasForeignKey(hm => hm.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Invitation entity
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasIndex(i => i.Token)
                .IsUnique();
        });
    }
}
