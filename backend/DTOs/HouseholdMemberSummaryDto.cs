namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for household member summary (for display in receipt UI)
/// </summary>
public class HouseholdMemberSummaryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
