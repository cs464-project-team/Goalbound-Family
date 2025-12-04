using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetByHouseholdMonthAsync(Guid householdId, int year, int month);
    Task<IEnumerable<Expense>> GetByUserMonthAsync(Guid userId, int year, int month);

    /// <summary>
    /// Get expenses for a user filtered by households they are currently members of
    /// </summary>
    Task<IEnumerable<Expense>> GetByUserMonthFilteredAsync(Guid userId, int year, int month, IEnumerable<Guid> userHouseholdIds);
    Task<int> GetCountByUserMonthAsync(Guid userId, int year, int month, string? category = null);
}