using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

/// <summary>
/// Represents assignment of a receipt item (or portion of it) to a household member
/// Allows splitting items between multiple family members
/// </summary>
public class ReceiptItemAssignment
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The receipt item being assigned
    /// </summary>
    [Required]
    public Guid ReceiptItemId { get; set; }

    /// <summary>
    /// The household member this is assigned to
    /// </summary>
    [Required]
    public Guid HouseholdMemberId { get; set; }

    /// <summary>
    /// Quantity assigned to this member (can be fractional, e.g., 0.5 for splitting)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal AssignedQuantity { get; set; } = 1;

    /// <summary>
    /// The base amount for this member's portion (before GST/Service Charge)
    /// Calculated as: (item.TotalPrice / item.Quantity) * AssignedQuantity
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseAmount { get; set; }

    /// <summary>
    /// Service charge applied to this member's portion
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceChargeAmount { get; set; } = 0;

    /// <summary>
    /// GST applied to this member's portion (after service charge)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal GstAmount { get; set; } = 0;

    /// <summary>
    /// Total amount for this assignment (BaseAmount + ServiceCharge + GST)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// When this assignment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// The receipt item being assigned
    /// </summary>
    [ForeignKey(nameof(ReceiptItemId))]
    public ReceiptItem ReceiptItem { get; set; } = null!;

    /// <summary>
    /// The household member this is assigned to
    /// </summary>
    [ForeignKey(nameof(HouseholdMemberId))]
    public HouseholdMember HouseholdMember { get; set; } = null!;
}
