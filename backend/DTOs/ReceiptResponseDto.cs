using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for receipt response (after OCR processing)
/// </summary>
public class ReceiptResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? HouseholdId { get; set; }
    public ReceiptStatus Status { get; set; }
    public string? MerchantName { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? OcrConfidence { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public List<ReceiptItemDto> Items { get; set; } = new();
    public List<HouseholdMemberSummaryDto> HouseholdMembers { get; set; } = new();
}
