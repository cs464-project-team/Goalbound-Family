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
    Task<ReceiptResponseDto> UploadReceiptAsync(ReceiptUploadDto uploadDto, Guid requestingUserId);

    /// <summary>
    /// Process receipt OCR without saving to database
    /// Returns parsed data for user review before persistence
    /// </summary>
    Task<ProcessReceiptOcrResponseDto> ProcessReceiptOcrOnlyAsync(ReceiptUploadDto uploadDto, Guid requestingUserId);

    /// <summary>
    /// Get receipt by ID
    /// </summary>
    Task<ReceiptResponseDto?> GetReceiptAsync(Guid receiptId, Guid requestingUserId);

    /// <summary>
    /// Get all receipts for a user
    /// </summary>
    Task<IEnumerable<ReceiptResponseDto>> GetUserReceiptsAsync(Guid userId, Guid requestingUserId);

    /// <summary>
    /// Get all receipts for a household
    /// </summary>
    Task<IEnumerable<ReceiptResponseDto>> GetHouseholdReceiptsAsync(Guid householdId, Guid requestingUserId);

    /// <summary>
    /// Add a manual item to a receipt
    /// </summary>
    Task<ReceiptItemDto> AddItemToReceiptAsync(AddReceiptItemDto addItemDto, Guid requestingUserId);

    /// <summary>
    /// Confirm receipt after user review
    /// </summary>
    Task<ReceiptResponseDto> ConfirmReceiptAsync(ConfirmReceiptDto confirmDto, Guid requestingUserId);

    /// <summary>
    /// Assign receipt items to household members with GST/Service Charge calculations
    /// and update member expenditures
    /// </summary>
    Task<ReceiptResponseDto> AssignItemsToMembersAsync(AssignReceiptItemsDto assignDto, Guid requestingUserId);
}
