using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

/// <summary>
/// Repository for receipt operations
/// </summary>
public interface IReceiptRepository : IRepository<Receipt>
{
    /// <summary>
    /// Get receipts by user ID with items included
    /// </summary>
    Task<IEnumerable<Receipt>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get receipt by ID with items included
    /// </summary>
    Task<Receipt?> GetByIdWithItemsAsync(Guid id);

    /// <summary>
    /// Add item to existing receipt
    /// </summary>
    Task<ReceiptItem> AddItemToReceiptAsync(ReceiptItem item);

    /// <summary>
    /// Update multiple items for a receipt
    /// </summary>
    Task UpdateReceiptItemsAsync(Guid receiptId, List<ReceiptItem> items);

    Task<int> GetReceiptCountByUserAndHouseholdAsync(Guid memberId, Guid householdId)
}
