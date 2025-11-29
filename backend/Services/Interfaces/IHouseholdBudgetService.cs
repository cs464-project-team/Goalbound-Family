using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IHouseholdBudgetService
{
    Task<IEnumerable<HouseholdBudgetDto>> GetBudgetsAsync(Guid householdId, int year, int month);
    Task<HouseholdBudgetDto> CreateOrUpdateAsync(CreateHouseholdBudgetRequest request);
    Task<bool> DeleteAsync(Guid id);
}