namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// Response DTO for OCR processing without saving to database
/// Contains parsed receipt data and image URL for later confirmation
/// </summary>
public class ProcessReceiptOcrResponseDto
{
    /// <summary>
    /// Image URL/path in storage
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// Original filename
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Merchant name extracted from OCR
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// Receipt date extracted from OCR
    /// </summary>
    public DateTime? ReceiptDate { get; set; }

    /// <summary>
    /// Total amount extracted from OCR
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Raw OCR text
    /// </summary>
    public string? RawOcrText { get; set; }

    /// <summary>
    /// OCR confidence score
    /// </summary>
    public decimal? OcrConfidence { get; set; }

    /// <summary>
    /// Parsed receipt items
    /// </summary>
    public List<ParsedReceiptItemDto> Items { get; set; } = new();

    /// <summary>
    /// Whether OCR processing was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Household members for assignment (if household ID was provided)
    /// </summary>
    public List<HouseholdMemberSummaryDto> HouseholdMembers { get; set; } = new();
}

/// <summary>
/// Parsed receipt item from OCR (before saving to database)
/// </summary>
public class ParsedReceiptItemDto
{
    /// <summary>
    /// Temporary ID for frontend tracking (not a database ID)
    /// </summary>
    public string TempId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Item name from OCR
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Total price for all quantities
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Line number from OCR
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Whether this item was manually added
    /// </summary>
    public bool IsManuallyAdded { get; set; }

    /// <summary>
    /// OCR confidence for this item
    /// </summary>
    public decimal OcrConfidence { get; set; }
}
