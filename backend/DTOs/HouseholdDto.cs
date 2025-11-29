namespace GoalboundFamily.Api.DTOs;

public class HouseholdDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Admin user (Parent)
    public Guid ParentId { get; set; }

    // Optional: number of members
    public int MemberCount { get; set; }
}

public class CreateHouseholdRequest
{
    public string Name { get; set; } = string.Empty;

    // Initial parent user ID
    public Guid ParentId { get; set; }
}

public class UpdateHouseholdRequest
{
    public string? Name { get; set; }
}