using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

/// <summary>
/// Service for receipt OCR and management
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// Upload and process a receipt with OCR
    /// </summary>
    Task<ReceiptResponseDto> UploadReceiptAsync(ReceiptUploadDto uploadDto);

    /// <summary>
    /// Get receipt by ID
    /// </summary>
    Task<ReceiptResponseDto?> GetReceiptAsync(Guid receiptId);

    /// <summary>
    /// Get all receipts for a user
    /// </summary>
    Task<IEnumerable<ReceiptResponseDto>> GetUserReceiptsAsync(Guid userId);

    /// <summary>
    /// Add a manual item to a receipt
    /// </summary>
    Task<ReceiptItemDto> AddItemToReceiptAsync(AddReceiptItemDto addItemDto);

    /// <summary>
    /// Confirm receipt after user review
    /// </summary>
    Task<ReceiptResponseDto> ConfirmReceiptAsync(ConfirmReceiptDto confirmDto);
}
