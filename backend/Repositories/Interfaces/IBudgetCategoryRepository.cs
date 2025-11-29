using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IBudgetCategoryRepository : IRepository<BudgetCategory>
{
    Task<IEnumerable<BudgetCategory>> GetByHouseholdAsync(Guid householdId);


    // Add this:
    Task<IEnumerable<BudgetCategory>> GetByIdsAsync(IEnumerable<Guid> ids);
}