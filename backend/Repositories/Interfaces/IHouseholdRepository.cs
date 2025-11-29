using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IHouseholdRepository : IRepository<Household>
{
    Task<Household?> GetWithMembersAsync(Guid id);
}