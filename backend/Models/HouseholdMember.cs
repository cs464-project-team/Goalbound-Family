using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

public class HouseholdMember
{
    [Key]
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }
    public Household? Household { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public string Role { get; set; } = "Member";
    // Roles: "Parent" or "Member"

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Expenditure tracking (in the household's currency)
    public decimal MonthlyExpenditure { get; set; } = 0;
    public decimal LifetimeExpenditure { get; set; } = 0;
    public DateTime? LastExpenditureUpdate { get; set; }

    // URL to avatar image
    public string Avatar { get; set; } = string.Empty;

    // Gamification
    public int Xp { get; set; } = 0;
    public int Streak { get; set; } = 0;

    // earned badges
    public ICollection<MemberBadge> MemberBadges { get; set; } = new List<MemberBadge>();
    public ICollection<MemberQuest> MemberQuests { get; set; } = new List<MemberQuest>();

    public int QuestsCompleted { get; set; } = 0;

    // Relationships (optional, depending on your schema)
    public ICollection<HouseholdMember> HouseholdMembers { get; set; } = new List<HouseholdMember>();
}