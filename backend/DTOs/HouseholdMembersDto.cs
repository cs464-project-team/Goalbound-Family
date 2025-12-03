using GoalboundFamily.Api.DTOs;

public class HouseholdMemberDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Combined name for UI display
    public string UserName { get; set; } = string.Empty;

    public string Role { get; set; } = "Member";
    public DateTime JoinedAt { get; set; }

    // Optional avatar
    public string Avatar { get; set; } = string.Empty;

    // Gamification
    public int Xp { get; set; } = 0;
    public int Streak { get; set; } = 0;
    public int QuestsCompleted { get; set; } = 0;

    // Badges
    public List<MemberBadgeDto> Badges { get; set; } = new List<MemberBadgeDto>();
}