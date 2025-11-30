using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalboundFamily.Api.Models;

/// <summary>
/// Receipt entity representing an uploaded receipt for OCR processing
/// </summary>
public class Receipt
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User who uploaded the receipt
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Path to the stored receipt image
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// Original filename of the uploaded image
    /// </summary>
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the receipt processing
    /// </summary>
    [Required]
    public ReceiptStatus Status { get; set; } = ReceiptStatus.Processing;

    /// <summary>
    /// Merchant/store name extracted from receipt
    /// </summary>
    [MaxLength(255)]
    public string? MerchantName { get; set; }

    /// <summary>
    /// Date on the receipt
    /// </summary>
    public DateTime? ReceiptDate { get; set; }

    /// <summary>
    /// Total amount on the receipt
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Raw OCR output text for debugging and reference
    /// </summary>
    public string? RawOcrText { get; set; }

    /// <summary>
    /// Average OCR confidence score (0-100)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? OcrConfidence { get; set; }

    /// <summary>
    /// Error message if OCR processing failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When the receipt was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the receipt was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties

    /// <summary>
    /// User who uploaded the receipt
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    /// <summary>
    /// Line items extracted from the receipt
    /// </summary>
    public ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
}
