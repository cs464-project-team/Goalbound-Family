using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class BudgetCategoryService : IBudgetCategoryService
{
    private readonly IBudgetCategoryRepository _repo;
    private readonly IHouseholdAuthorizationService _authService;

    public BudgetCategoryService(IBudgetCategoryRepository repo, IHouseholdAuthorizationService authService)
    {
        _repo = repo;
        _authService = authService;
    }

    public async Task<IEnumerable<BudgetCategoryDto>> GetCategoriesAsync(Guid householdId, Guid requestingUserId)
    {
        // Verify user is in the household
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, householdId);

        var cats = await _repo.GetByHouseholdAsync(householdId);
        return cats.Select(c => new BudgetCategoryDto { Id = c.Id, Name = c.Name });
    }

    public async Task<BudgetCategoryDto> CreateAsync(CreateBudgetCategoryRequest request, Guid requestingUserId)
    {
        // Verify user is in the household
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, request.HouseholdId);

        var cat = new BudgetCategory { HouseholdId = request.HouseholdId, Name = request.Name };
        await _repo.AddAsync(cat);
        await _repo.SaveChangesAsync();
        return new BudgetCategoryDto { Id = cat.Id, Name = cat.Name };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid requestingUserId)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat == null) return false;

        // Verify user is in the household that owns this category
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, cat.HouseholdId);

        await _repo.DeleteAsync(cat);
        await _repo.SaveChangesAsync();
        return true;
    }
}