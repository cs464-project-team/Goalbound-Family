using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly IHouseholdBudgetRepository _budgetRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly IBudgetCategoryRepository _categoryRepo;

    public DashboardService(
        IHouseholdBudgetRepository budgetRepo,
        IExpenseRepository expenseRepo,
        IBudgetCategoryRepository categoryRepo)
    {
        _budgetRepo = budgetRepo;
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<DashboardSummaryDto> GetHouseholdMonthlySummaryAsync(Guid householdId, int year, int month)
    {
        var budgets = (await _budgetRepo.GetByHouseholdMonthAsync(householdId, year, month)).ToList();
        var expenses = (await _expenseRepo.GetByHouseholdMonthAsync(householdId, year, month)).ToList();

        // Map category id -> name
        var categoryIds = budgets.Select(b => b.CategoryId).Union(expenses.Select(e => e.CategoryId)).Distinct();
        var categories = (await _categoryRepo.GetByIdsAsync(categoryIds))
            .ToDictionary(c => c.Id, c => c.Name);

        var categorySummaries = new List<CategorySummaryDto>();

        // For budgets (ensures categories with budgets appear even if no spend)
        foreach (var b in budgets)
        {
            var spent = expenses.Where(e => e.CategoryId == b.CategoryId).Sum(e => e.Amount);
            categories.TryGetValue(b.CategoryId, out var catName);

            categorySummaries.Add(new CategorySummaryDto
            {
                CategoryId = b.CategoryId,
                CategoryName = catName ?? string.Empty,
                BudgetLimit = b.Limit,
                Spent = spent
            });
        }

        // For expenses with no budget
        var budgetedCategoryIds = new HashSet<Guid>(budgets.Select(b => b.CategoryId));
        var expenseOnlyCategoryIds = expenses.Select(e => e.CategoryId).Where(id => !budgetedCategoryIds.Contains(id)).Distinct();
        foreach (var cId in expenseOnlyCategoryIds)
        {
            var spent = expenses.Where(e => e.CategoryId == cId).Sum(e => e.Amount);
            categories.TryGetValue(cId, out var catName);

            categorySummaries.Add(new CategorySummaryDto
            {
                CategoryId = cId,
                CategoryName = catName ?? string.Empty,
                BudgetLimit = 0,
                Spent = spent
            });
        }

        return new DashboardSummaryDto
        {
            HouseholdId = householdId,
            Year = year,
            Month = month,
            Categories = categorySummaries
        };
    }
}