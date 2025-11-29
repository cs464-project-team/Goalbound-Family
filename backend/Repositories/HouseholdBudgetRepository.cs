using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class HouseholdBudgetRepository : Repository<HouseholdBudget>, IHouseholdBudgetRepository
{
    public HouseholdBudgetRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<HouseholdBudget>> GetByHouseholdMonthAsync(Guid householdId, int year, int month)
    {
        return await _dbSet.Include(b => b.Category)
            .Where(b => b.HouseholdId == householdId && b.Year == year && b.Month == month)
            .ToListAsync();
    }

    public async Task<HouseholdBudget?> GetByHouseholdCategoryMonthAsync(Guid householdId, Guid categoryId, int year, int month)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.HouseholdId == householdId && b.CategoryId == categoryId && b.Year == year && b.Month == month);
    }
}