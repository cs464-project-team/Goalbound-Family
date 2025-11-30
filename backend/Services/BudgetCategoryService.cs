using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class BudgetCategoryService : IBudgetCategoryService
{
    private readonly IBudgetCategoryRepository _repo;

    public BudgetCategoryService(IBudgetCategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<BudgetCategoryDto>> GetCategoriesAsync(Guid householdId)
    {
        var cats = await _repo.GetByHouseholdAsync(householdId);
        return cats.Select(c => new BudgetCategoryDto { Id = c.Id, Name = c.Name });
    }

    public async Task<BudgetCategoryDto> CreateAsync(CreateBudgetCategoryRequest request)
    {
        var cat = new BudgetCategory { HouseholdId = request.HouseholdId, Name = request.Name };
        await _repo.AddAsync(cat);
        await _repo.SaveChangesAsync();
        return new BudgetCategoryDto { Id = cat.Id, Name = cat.Name };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat == null) return false;
        await _repo.DeleteAsync(cat);
        await _repo.SaveChangesAsync();
        return true;
    }
}