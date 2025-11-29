using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IHouseholdBudgetRepository : IRepository<HouseholdBudget>
{
    Task<IEnumerable<HouseholdBudget>> GetByHouseholdMonthAsync(Guid householdId, int year, int month);
    Task<HouseholdBudget?> GetByHouseholdCategoryMonthAsync(Guid householdId, Guid categoryId, int year, int month);
}