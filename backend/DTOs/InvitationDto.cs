public class InvitationDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public Guid HouseholdId { get; set; }
    public string HouseholdName { get; set; } = string.Empty;

    public Guid InvitedByUserId { get; set; }
    public string InvitedByName { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
}

public class CreateInvitationRequest
{
    public string Email { get; set; } = string.Empty;
    public Guid HouseholdId { get; set; }
    public Guid InvitedByUserId { get; set; }
}

public class AcceptInvitationRequest
{
    public string Token { get; set; } = string.Empty;

    // The new user who is accepting the invitation (Supabase UUID)
    public Guid UserId { get; set; }
}