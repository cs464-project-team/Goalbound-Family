namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for a receipt line item
/// </summary>
public class ReceiptItemDto
{
    public Guid? Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int LineNumber { get; set; }
    public bool IsManuallyAdded { get; set; }
    public decimal? OcrConfidence { get; set; }
}
