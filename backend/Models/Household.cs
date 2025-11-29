using System.ComponentModel.DataAnnotations;
namespace GoalboundFamily.Api.Models;

public class Household
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Parent / Admin (Automatically the first user)
    public Guid ParentId { get; set; }
    public User? Parent { get; set; }

    // Members
    public ICollection<HouseholdMember> Members { get; set; } 
        = new List<HouseholdMember>();
}