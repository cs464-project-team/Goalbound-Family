using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class HouseholdBudgetService : IHouseholdBudgetService
{
    private readonly IHouseholdBudgetRepository _repo;
    private readonly IBudgetCategoryRepository _categoryRepo;

    public HouseholdBudgetService(
        IHouseholdBudgetRepository repo,
        IBudgetCategoryRepository categoryRepo)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
    }

    public async Task<IEnumerable<HouseholdBudgetDto>> GetBudgetsAsync(Guid householdId, int year, int month)
    {
        var budgets = await _repo.GetByHouseholdMonthAsync(householdId, year, month);
        return budgets.Select(b => new HouseholdBudgetDto
        {
            Id = b.Id,
            HouseholdId = b.HouseholdId,
            CategoryId = b.CategoryId,
            CategoryName = b.Category?.Name ?? string.Empty,
            Limit = b.Limit,
            Year = b.Year,
            Month = b.Month
        });
    }

    public async Task<HouseholdBudgetDto> CreateOrUpdateAsync(CreateHouseholdBudgetRequest request)
    {
        var existing = await _repo.GetByHouseholdCategoryMonthAsync(
            request.HouseholdId,
            request.CategoryId,
            request.Year,
            request.Month);

        if (existing != null)
        {
            existing.Limit = request.Limit;
            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();
            return new HouseholdBudgetDto
            {
                Id = existing.Id,
                HouseholdId = existing.HouseholdId,
                CategoryId = existing.CategoryId,
                CategoryName = existing.Category?.Name ?? string.Empty,
                Limit = existing.Limit,
                Year = existing.Year,
                Month = existing.Month
            };
        }

        var newBudget = new HouseholdBudget
        {
            HouseholdId = request.HouseholdId,
            CategoryId = request.CategoryId,
            Limit = request.Limit,
            Year = request.Year,
            Month = request.Month
        };

        await _repo.AddAsync(newBudget);
        await _repo.SaveChangesAsync();

        return new HouseholdBudgetDto
        {
            Id = newBudget.Id,
            HouseholdId = newBudget.HouseholdId,
            CategoryId = newBudget.CategoryId,
            CategoryName = (await _categoryRepo.GetByIdAsync(newBudget.CategoryId))?.Name ?? string.Empty,
            Limit = newBudget.Limit,
            Year = newBudget.Year,
            Month = newBudget.Month
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var b = await _repo.GetByIdAsync(id);
        if (b == null) return false;
        await _repo.DeleteAsync(b);
        await _repo.SaveChangesAsync();
        return true;
    }
}