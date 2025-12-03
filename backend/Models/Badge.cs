using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

public class Badge
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // optional icon / image path
    public string Icon { get; set; } = string.Empty;

    // everyone who earned this badge
    public ICollection<MemberBadge> MemberBadges { get; set; } = new List<MemberBadge>();
}


