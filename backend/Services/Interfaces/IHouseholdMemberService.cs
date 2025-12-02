using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IHouseholdMemberService
{
    Task<IEnumerable<HouseholdMemberDto>> GetMembersAsync(Guid householdId);
    Task<bool> AddMemberAsync(Guid householdId, Guid userId, string role);
    Task<bool> RemoveMemberAsync(Guid memberId);
    Task<IEnumerable<HouseholdDto>> GetHouseholdsForUserAsync(Guid userId);
}