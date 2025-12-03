namespace GoalboundFamily.Api.Models;

/// <summary>
/// Status of receipt processing
/// </summary>
public enum ReceiptStatus
{
    /// <summary>
    /// OCR is currently processing the receipt
    /// </summary>
    Processing = 0,

    /// <summary>
    /// OCR complete, waiting for user review/confirmation
    /// </summary>
    ReviewRequired = 1,

    /// <summary>
    /// User has reviewed and confirmed the receipt items
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// OCR or processing failed
    /// </summary>
    Failed = 3
}