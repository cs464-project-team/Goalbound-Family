using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

/// <summary>
/// Repository implementation for receipt operations
/// </summary>
public class ReceiptRepository : Repository<Receipt>, IReceiptRepository
{
    public ReceiptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Receipt>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Receipts
            .Include(r => r.Items)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();
    }

    public async Task<Receipt?> GetByIdWithItemsAsync(Guid id)
    {
        return await _context.Receipts
            .Include(r => r.Items.OrderBy(i => i.LineNumber))
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ReceiptItem> AddItemToReceiptAsync(ReceiptItem item)
    {
        _context.ReceiptItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateReceiptItemsAsync(Guid receiptId, List<ReceiptItem> items)
    {
        // Remove existing items
        var existingItems = await _context.ReceiptItems
            .Where(i => i.ReceiptId == receiptId)
            .ToListAsync();

        _context.ReceiptItems.RemoveRange(existingItems);

        // Add new items
        _context.ReceiptItems.AddRange(items);

        await _context.SaveChangesAsync();
    }

    // Count receipts for progress calculation
    public async Task<int> GetReceiptCountByUserAndHouseholdAsync(Guid userId, Guid householdId)
    {
        return await _context.Receipts
            .Where(r => r.UserId == userId && r.HouseholdId == householdId)
            .CountAsync();
    }
}
