using System.ComponentModel.DataAnnotations;
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
}