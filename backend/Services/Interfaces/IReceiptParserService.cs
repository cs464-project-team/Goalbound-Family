namespace GoalboundFamily.Api.Services.Interfaces;

/// <summary>
/// Parsed line item from a receipt
/// </summary>
public class ParsedReceiptItem
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int LineNumber { get; set; }
    public decimal Confidence { get; set; }
}

/// <summary>
/// Complete parsed receipt
/// </summary>
public class ParsedReceipt
{
    public string? MerchantName { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<ParsedReceiptItem> Items { get; set; } = new();

    /// <summary>
    /// Sum of all parsed item prices
    /// </summary>
    public decimal CalculatedTotal => Items.Sum(i => i.TotalPrice);

    /// <summary>
    /// Difference between calculated total and receipt total (null if receipt total not found)
    /// Positive = parsed items exceed receipt total (possible false positives)
    /// Negative = parsed items less than receipt total (missing items)
    /// </summary>
    public decimal? TotalDiscrepancy => TotalAmount.HasValue ? CalculatedTotal - TotalAmount.Value : null;

    /// <summary>
    /// True if calculated total matches receipt total within $0.10 tolerance
    /// </summary>
    public bool TotalMatchesReceipt => TotalDiscrepancy.HasValue && Math.Abs(TotalDiscrepancy.Value) < 0.11m;
}

/// <summary>
/// Service for parsing receipt text into structured data
/// </summary>
public interface IReceiptParserService
{
    /// <summary>
    /// Parse OCR text into structured receipt data
    /// </summary>
    /// <param name="ocrResult">OCR result containing text and confidence</param>
    /// <returns>Parsed receipt with line items</returns>
    Task<ParsedReceipt> ParseReceiptAsync(OcrResult ocrResult);
}
