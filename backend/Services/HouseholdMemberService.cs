using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class HouseholdMemberService : IHouseholdMemberService
{
    private readonly IHouseholdMemberRepository _memberRepo;

    public HouseholdMemberService(IHouseholdMemberRepository memberRepo)
    {
        _memberRepo = memberRepo;
    }

    public async Task<IEnumerable<HouseholdMemberDto>> GetMembersAsync(Guid householdId)
    {
        var members = await _memberRepo.FindAsync(m => m.HouseholdId == householdId);

        return members.Select(m => new HouseholdMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            FirstName = m.User?.FirstName ?? "",
            LastName = m.User?.LastName ?? "",
            Email = m.User?.Email ?? "",
            Role = m.Role,
            JoinedAt = m.JoinedAt,
            Avatar = m.Avatar,
            Xp = m.Xp,
            Streak = m.Streak,
            QuestsCompleted = m.QuestsCompleted
        });
    }

    public async Task<IEnumerable<HouseholdDto>> GetHouseholdsForUserAsync(Guid userId)
    {
        var memberships = await _memberRepo.GetByUserIdAsync(userId);
        return memberships
            .Where(m => m.Household != null)
            .Select(m => new HouseholdDto
            {
                Id = m.Household!.Id,
                Name = m.Household.Name,
                ParentId = m.Household.ParentId,
                MemberCount = m.Household.Members?.Count ?? 0
            });
    }

    public async Task<bool> AddMemberAsync(Guid householdId, Guid userId, string role)
    {
        if (await _memberRepo.IsUserInHouseholdAsync(userId, householdId))
            return false;

        var member = new HouseholdMember
        {
            HouseholdId = householdId,
            UserId = userId,
            Role = role
        };

        await _memberRepo.AddAsync(member);
        await _memberRepo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid memberId)
    {
        var member = await _memberRepo.GetByIdAsync(memberId);
        if (member == null) return false;

        await _memberRepo.DeleteAsync(member);
        await _memberRepo.SaveChangesAsync();

        return true;
    }

    public async Task<HouseholdMemberDto?> GetByUserAndHouseholdAsync(Guid userId, Guid householdId)
    {
        var member = await _memberRepo.GetByUserAndHouseholdAsync(userId, householdId);
        if (member == null) return null;

        return new HouseholdMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            FirstName = member.User?.FirstName ?? "",
            LastName = member.User?.LastName ?? "",
            Email = member.User?.Email ?? "",
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }
}