using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Services;

/// <summary>
/// Service for authorizing user access to household-related resources
/// Prevents former household members from accessing data after they leave
/// </summary>
public class HouseholdAuthorizationService : IHouseholdAuthorizationService
{
    private readonly ApplicationDbContext _context;

    public HouseholdAuthorizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsUserInHouseholdAsync(Guid userId, Guid householdId)
    {
        return await _context.HouseholdMembers
            .AnyAsync(m => m.UserId == userId && m.HouseholdId == householdId);
    }

    public async Task<bool> CanAccessExpenseAsync(Guid userId, Guid expenseId)
    {
        var expense = await _context.Expenses
            .AsNoTracking()
            .Select(e => new { e.Id, e.HouseholdId })
            .FirstOrDefaultAsync(e => e.Id == expenseId);

        if (expense == null)
        {
            return false;
        }

        return await IsUserInHouseholdAsync(userId, expense.HouseholdId);
    }

    public async Task<bool> CanAccessReceiptAsync(Guid userId, Guid receiptId)
    {
        var receipt = await _context.Receipts
            .AsNoTracking()
            .Select(r => new { r.Id, r.HouseholdId, r.UserId })
            .FirstOrDefaultAsync(r => r.Id == receiptId);

        if (receipt == null)
        {
            return false;
        }

        // User can access if they uploaded it OR they're in the household
        if (receipt.UserId == userId)
        {
            return true;
        }

        if (receipt.HouseholdId.HasValue)
        {
            return await IsUserInHouseholdAsync(userId, receipt.HouseholdId.Value);
        }

        return false;
    }

    public async Task<IEnumerable<Guid>> GetUserHouseholdIdsAsync(Guid userId)
    {
        return await _context.HouseholdMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.HouseholdId)
            .ToListAsync();
    }

    public async Task ValidateHouseholdAccessAsync(Guid userId, Guid householdId)
    {
        if (!await IsUserInHouseholdAsync(userId, householdId))
        {
            throw new UnauthorizedAccessException($"User {userId} is not a member of household {householdId}");
        }
    }

    public async Task ValidateExpenseAccessAsync(Guid userId, Guid expenseId)
    {
        if (!await CanAccessExpenseAsync(userId, expenseId))
        {
            throw new UnauthorizedAccessException($"User {userId} cannot access expense {expenseId}");
        }
    }

    public async Task ValidateReceiptAccessAsync(Guid userId, Guid receiptId)
    {
        if (!await CanAccessReceiptAsync(userId, receiptId))
        {
            throw new UnauthorizedAccessException($"User {userId} cannot access receipt {receiptId}");
        }
    }
}
