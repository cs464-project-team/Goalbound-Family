using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

/// <summary>
/// Individual line item from a receipt
/// </summary>
public class ReceiptItem
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Parent receipt ID
    /// </summary>
    [Required]
    public Guid ReceiptId { get; set; }

    /// <summary>
    /// Item name/description
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of the item
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Price per unit (if available)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Total price for this line item
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Order/position of this item in the receipt
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// True if user manually added this item (not from OCR)
    /// </summary>
    public bool IsManuallyAdded { get; set; } = false;

    /// <summary>
    /// OCR confidence for this specific item (0-100)
    /// Null if manually added
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? OcrConfidence { get; set; }

    /// <summary>
    /// When the item was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Parent receipt
    /// </summary>
    [ForeignKey(nameof(ReceiptId))]
    public Receipt Receipt { get; set; } = null!;

    /// <summary>
    /// Assignments of this item to household members
    /// </summary>
    public ICollection<ReceiptItemAssignment> Assignments { get; set; } = new List<ReceiptItemAssignment>();
}
