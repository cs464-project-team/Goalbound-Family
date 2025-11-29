using System.ComponentModel.DataAnnotations;
namespace GoalboundFamily.Api.Models;


public class Invitation
{
    [Key]
    public Guid Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Guid HouseholdId { get; set; }
    public Household? Household { get; set; }

    [Required]
    public Guid InvitedByUserId { get; set; }
    public User? InvitedByUser { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; } 
        = DateTime.UtcNow.AddDays(7);

    public bool IsAccepted { get; set; } = false;
}