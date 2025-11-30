using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IBudgetCategoryService
{
    Task<IEnumerable<BudgetCategoryDto>> GetCategoriesAsync(Guid householdId);
    Task<BudgetCategoryDto> CreateAsync(CreateBudgetCategoryRequest request);
    Task<bool> DeleteAsync(Guid id);
}