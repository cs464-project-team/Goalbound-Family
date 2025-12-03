using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IHouseholdMemberRepository : IRepository<HouseholdMember>
{
    Task<bool> IsUserInHouseholdAsync(Guid userId, Guid householdId);
    Task<IEnumerable<HouseholdMember>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<HouseholdMember>> GetByHouseholdIdAsync(Guid householdId);
    Task<HouseholdMember?> GetByUserAndHouseholdAsync(Guid userId, Guid householdId);
    Task<IEnumerable<HouseholdMember>> GetWithIncludesAsync(Guid householdId);
}