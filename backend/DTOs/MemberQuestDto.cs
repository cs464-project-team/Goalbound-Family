namespace GoalboundFamily.Api.DTOs;

public class MemberQuestDto
{
    public Guid HouseholdMemberId { get; set; }
    public Guid QuestId { get; set; }

    // MemberQuest fields
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }

    // Quest fields
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int XpReward { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int Target { get; set; }
}
