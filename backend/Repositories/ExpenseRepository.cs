using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Expense>> GetByHouseholdMonthAsync(Guid householdId, int year, int month)
    {
        return await _dbSet
            .Include(e => e.Category)
            .Include(e => e.Household)
            .Where(e => e.HouseholdId == householdId && e.Date.Year == year && e.Date.Month == month)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByUserMonthAsync(Guid userId, int year, int month)
    {
        return await _dbSet
            .Include(e => e.Category)
            .Include(e => e.Household)
            .Where(e => e.UserId == userId && e.Date.Year == year && e.Date.Month == month)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByUserMonthFilteredAsync(Guid userId, int year, int month, IEnumerable<Guid> userHouseholdIds)
    {
        var householdIdsList = userHouseholdIds.ToList();
        return await _dbSet
            .Include(e => e.Category)
            .Include(e => e.Household)
            .Where(e => e.UserId == userId
                && e.Date.Year == year
                && e.Date.Month == month
                && householdIdsList.Contains(e.HouseholdId))
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }
}