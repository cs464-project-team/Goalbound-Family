using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

public class MemberQuest
{
    public Guid HouseholdMemberId { get; set; }
    public Guid QuestId { get; set; }

    // "in-progress" | "completed" | "claimed"
    public string Status { get; set; } = "in-progress";

    public int Progress { get; set; } = 0;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // For timed quests
    public DateTime? StartTime { get; set; }

    // "completed" timestamp (before claiming)
    public DateTime? CompletedAt { get; set; }

    // When rewards were claimed
    public DateTime? ClaimedAt { get; set; }

    public HouseholdMember? HouseholdMember { get; set; }
    public Quest? Quest { get; set; }
}
