using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IHouseholdMemberRepository : IRepository<HouseholdMember>
{
    Task<bool> IsUserInHouseholdAsync(Guid userId, Guid householdId);
}