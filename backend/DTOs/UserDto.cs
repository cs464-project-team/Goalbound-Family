namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// Example DTO - User data transfer object for API responses
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Household memberships
    public List<HouseholdMemberDto>? Households { get; set; }
}

/// <summary>
/// Example DTO - Create user request
/// </summary>
public class CreateUserRequest
{
    public Guid Id { get; set; } // Supabase Auth UUID
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Optional: Household name during first registration
    public string? HouseholdName { get; set; }

}

/// <summary>
/// Example DTO - Update user request
/// </summary>
public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
