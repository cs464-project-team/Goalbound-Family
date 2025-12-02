using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for assigning receipt items to household members
/// Used when confirming a receipt
/// Supports both existing receipts (ReceiptId provided) and new receipts (receipt metadata provided)
/// </summary>
public class AssignReceiptItemsDto
{
    /// <summary>
    /// Optional: Receipt ID if the receipt already exists in the database
    /// If null, a new receipt will be created using the provided metadata
    /// </summary>
    public Guid? ReceiptId { get; set; }

    /// <summary>
    /// Required for new receipts: User who uploaded the receipt
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Required for new receipts: Image URL/path from storage
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Optional: Original filename
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Optional: Merchant name from OCR
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// Optional: Receipt date from OCR
    /// </summary>
    public DateTime? ReceiptDate { get; set; }

    /// <summary>
    /// Optional: Total amount from OCR
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Optional: Raw OCR text
    /// </summary>
    public string? RawOcrText { get; set; }

    /// <summary>
    /// Optional: OCR confidence score
    /// </summary>
    public decimal? OcrConfidence { get; set; }

    [Required]
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Budget category ID for the expense
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }

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
    /// For new receipts, includes item details
    /// For existing receipts, references existing item IDs
    /// </summary>
    [Required]
    public List<ItemAssignmentDto> ItemAssignments { get; set; } = new();
}

/// <summary>
/// Assignment of a receipt item to household members
/// Supports both existing items (ReceiptItemId) and new items (item details provided)
/// </summary>
public class ItemAssignmentDto
{
    /// <summary>
    /// Optional: Receipt item ID if referencing an existing item
    /// If null, a new item will be created using the provided details
    /// </summary>
    public Guid? ReceiptItemId { get; set; }

    /// <summary>
    /// Required for new items: Item name (potentially edited by user)
    /// </summary>
    public string? ItemName { get; set; }

    /// <summary>
    /// Required for new items: Quantity
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Required for new items: Total price for all quantities
    /// </summary>
    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// Optional: Unit price (can be calculated from TotalPrice / Quantity)
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Optional: Line number from OCR
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Optional: Whether this item was manually added
    /// </summary>
    public bool? IsManuallyAdded { get; set; }

    /// <summary>
    /// Optional: OCR confidence for this item
    /// </summary>
    public decimal? OcrConfidence { get; set; }

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
