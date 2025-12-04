using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IBudgetCategoryService
{
    Task<IEnumerable<BudgetCategoryDto>> GetCategoriesAsync(Guid householdId, Guid requestingUserId);
    Task<BudgetCategoryDto> CreateAsync(CreateBudgetCategoryRequest request, Guid requestingUserId);
    Task<bool> DeleteAsync(Guid id, Guid requestingUserId);
}