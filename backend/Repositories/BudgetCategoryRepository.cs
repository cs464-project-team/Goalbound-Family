using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class BudgetCategoryRepository : Repository<BudgetCategory>, IBudgetCategoryRepository
{
    public BudgetCategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<BudgetCategory>> GetByHouseholdAsync(Guid householdId)
    {
        return await _dbSet.Where(c => c.HouseholdId == householdId).OrderBy(c => c.Name).ToListAsync();
    }


    public async Task<IEnumerable<BudgetCategory>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
    return await _dbSet
        .Where(c => ids.Contains(c.Id))
        .ToListAsync();
    }
}

