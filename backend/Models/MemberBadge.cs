using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

public class MemberBadge
{
    public Guid HouseholdMemberId { get; set; }
    public Guid BadgeId { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    public HouseholdMember? HouseholdMember { get; set; }
    public Badge? Badge { get; set; }
}