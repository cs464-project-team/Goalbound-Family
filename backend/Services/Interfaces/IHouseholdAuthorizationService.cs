namespace GoalboundFamily.Api.Services.Interfaces;

/// <summary>
/// Service for authorizing user access to household-related resources
/// </summary>
public interface IHouseholdAuthorizationService
{
    /// <summary>
    /// Check if a user is an active member of a household
    /// </summary>
    Task<bool> IsUserInHouseholdAsync(Guid userId, Guid householdId);

    /// <summary>
    /// Check if a user can access a specific expense
    /// </summary>
    Task<bool> CanAccessExpenseAsync(Guid userId, Guid expenseId);

    /// <summary>
    /// Check if a user can access a specific receipt
    /// </summary>
    Task<bool> CanAccessReceiptAsync(Guid userId, Guid receiptId);

    /// <summary>
    /// Get all household IDs that a user is an active member of
    /// </summary>
    Task<IEnumerable<Guid>> GetUserHouseholdIdsAsync(Guid userId);

    /// <summary>
    /// Verify user has access to household and throw if not
    /// </summary>
    Task ValidateHouseholdAccessAsync(Guid userId, Guid householdId);

    /// <summary>
    /// Verify user has access to expense and throw if not
    /// </summary>
    Task ValidateExpenseAccessAsync(Guid userId, Guid expenseId);

    /// <summary>
    /// Verify user has access to receipt and throw if not
    /// </summary>
    Task ValidateReceiptAccessAsync(Guid userId, Guid receiptId);
}
