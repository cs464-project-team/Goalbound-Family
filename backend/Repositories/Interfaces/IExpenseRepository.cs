using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetByHouseholdMonthAsync(Guid householdId, int year, int month);
    Task<IEnumerable<Expense>> GetByUserMonthAsync(Guid userId, int year, int month);
}