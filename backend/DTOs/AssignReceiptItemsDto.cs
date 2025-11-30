using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for assigning receipt items to household members
/// Used when confirming a receipt
/// </summary>
public class AssignReceiptItemsDto
{
    [Required]
    public Guid ReceiptId { get; set; }

    [Required]
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Whether to apply 10% service charge
    /// </summary>
    public bool ApplyServiceCharge { get; set; }

    /// <summary>
    /// Whether to apply 9% GST
    /// </summary>
    public bool ApplyGst { get; set; }

    /// <summary>
    /// Item assignments - each item can have multiple assignments (for splitting)
    /// </summary>
    [Required]
    public List<ItemAssignmentDto> ItemAssignments { get; set; } = new();
}

/// <summary>
/// Assignment of a receipt item to household members
/// </summary>
public class ItemAssignmentDto
{
    [Required]
    public Guid ReceiptItemId { get; set; }

    /// <summary>
    /// List of household member assignments for this item
    /// The sum of AssignedQuantity should equal the item's Quantity
    /// </summary>
    [Required]
    public List<MemberAssignmentDto> MemberAssignments { get; set; } = new();
}

/// <summary>
/// Individual member assignment within an item
/// </summary>
public class MemberAssignmentDto
{
    [Required]
    public Guid HouseholdMemberId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal AssignedQuantity { get; set; }
}
