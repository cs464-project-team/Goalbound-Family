using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

public class Quest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Type { get; set; } = "daily";
    // "daily" | "weekly" | "timed"

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int XpReward { get; set; }

    public int Target { get; set; }  // required amount to complete

    public string Difficulty { get; set; } = "easy";
    // "easy" | "medium" | "hard"

    public string Category { get; set; } = "finance";
    // "finance" | "food" | "health" | "productivity"

    // Default time limit for timed quests (null = unlimited)
    public int? TimeLimitSeconds { get; set; }

    // Optional: is this quest repeatable?
    public bool IsRepeatable { get; set; } = false;

    public ICollection<MemberQuest> MemberQuests { get; set; } = new List<MemberQuest>();
}
