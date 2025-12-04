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
    public DbSet<ReceiptItemAssignment> ReceiptItemAssignments { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<BudgetCategory> BudgetCategories { get; set; }
    public DbSet<HouseholdBudget> HouseholdBudgets { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Quest> Quests { get; set; }
    public DbSet<MemberQuest> MemberQuests { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<MemberBadge> MemberBadges { get; set; }

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
            entity.HasIndex(r => r.HouseholdId);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.UploadedAt);

            entity.Property(r => r.UploadedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Household)
                .WithMany()
                .HasForeignKey(r => r.HouseholdId)
                .OnDelete(DeleteBehavior.SetNull);
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

        // Configure ReceiptItemAssignment entity
        modelBuilder.Entity<ReceiptItemAssignment>(entity =>
        {
            entity.HasIndex(ria => ria.ReceiptItemId);
            entity.HasIndex(ria => ria.HouseholdMemberId);
            entity.HasIndex(ria => new { ria.ReceiptItemId, ria.HouseholdMemberId });

            entity.Property(ria => ria.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(ria => ria.ReceiptItem)
                .WithMany(ri => ri.Assignments)
                .HasForeignKey(ria => ria.ReceiptItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ria => ria.HouseholdMember)
                .WithMany()
                .HasForeignKey(ria => ria.HouseholdMemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Add more entity configurations as needed
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

        // MemberQuest composite key + relationships
        modelBuilder.Entity<MemberQuest>()
            .HasKey(mq => new { mq.HouseholdMemberId, mq.QuestId });

        modelBuilder.Entity<MemberQuest>()
            .HasOne(mq => mq.HouseholdMember)
            .WithMany(hm => hm.MemberQuests)
            .HasForeignKey(mq => mq.HouseholdMemberId);

        modelBuilder.Entity<MemberQuest>()
            .HasOne(mq => mq.Quest)
            .WithMany(q => q.MemberQuests)
            .HasForeignKey(mq => mq.QuestId);

        // MemberBadge composite key + relationships
        modelBuilder.Entity<MemberBadge>()
            .HasKey(mb => new { mb.HouseholdMemberId, mb.BadgeId });

        modelBuilder.Entity<MemberBadge>()
            .HasOne(mb => mb.HouseholdMember)
            .WithMany(hm => hm.MemberBadges)
            .HasForeignKey(mb => mb.HouseholdMemberId);

        modelBuilder.Entity<MemberBadge>()
            .HasOne(mb => mb.Badge)
            .WithMany(b => b.MemberBadges)
            .HasForeignKey(mb => mb.BadgeId);
    }
}
