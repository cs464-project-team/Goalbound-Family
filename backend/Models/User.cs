using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.Models;

/// <summary>
/// Example entity - User model
/// </summary>
public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for relationships
    // Example: public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
