using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;
using GoalboundFamily.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Services;

public class HouseholdService : IHouseholdService
{
    private readonly IHouseholdRepository _houseRepo;
    private readonly IHouseholdMemberRepository _memberRepo;
    private readonly ApplicationDbContext _dbContext;

    public HouseholdService(
        IHouseholdRepository houseRepo,
        IHouseholdMemberRepository memberRepo,
        ApplicationDbContext dbContext)
    {
        _houseRepo = houseRepo;
        _memberRepo = memberRepo;
        _dbContext = dbContext;
    }

    public async Task<HouseholdDto?> GetAsync(Guid id)
    {
        var household = await _houseRepo.GetWithMembersAsync(id);
        if (household == null) return null;

        return new HouseholdDto
        {
            Id = household.Id,
            Name = household.Name,
            ParentId = household.ParentId,
            MemberCount = household.Members.Count
        };
    }

    public async Task<IEnumerable<HouseholdDto>> GetAllAsync()
    {
        var households = await _houseRepo.GetAllAsync();
        return households.Select(h => new HouseholdDto
        {
            Id = h.Id,
            Name = h.Name,
            ParentId = h.ParentId,
            MemberCount = 0
        });
    }

    public async Task<IEnumerable<HouseholdDto>> GetByUserIdAsync(Guid userId)
    {
        var households = await _dbContext.HouseholdMembers
            .Where(hm => hm.UserId == userId)
            .Include(hm => hm.Household)
                .ThenInclude(h => h.Members)
            .Select(hm => hm.Household)
            .Distinct()
            .ToListAsync();

        return households.Select(h => new HouseholdDto
        {
            Id = h.Id,
            Name = h.Name,
            ParentId = h.ParentId,
            MemberCount = h.Members.Count
        });
    }

    public async Task<IEnumerable<HouseholdMemberDto>> GetMembersAsync(Guid householdId)
    {
        var members = await _dbContext.HouseholdMembers
            .Where(hm => hm.HouseholdId == householdId)
            .Include(hm => hm.User)
            .ToListAsync();

        return members.Select(m => new HouseholdMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            FirstName = m.User?.FirstName ?? string.Empty,
            LastName = m.User?.LastName ?? string.Empty,
            Email = m.User?.Email ?? string.Empty,
            UserName = $"{m.User?.FirstName ?? ""} {m.User?.LastName ?? ""}".Trim(),
            Role = m.Role,
            JoinedAt = m.JoinedAt
        });
    }

    public async Task<HouseholdDto> CreateAsync(CreateHouseholdRequest request)
    {
        var household = new Household
        {
            Name = request.Name,
            ParentId = request.ParentId
        };

        await _houseRepo.AddAsync(household);

        // Add parent as member
        var parentMember = new HouseholdMember
        {
            HouseholdId = household.Id,
            UserId = request.ParentId,
            Role = "Parent"
        };

        await _memberRepo.AddAsync(parentMember);

        await _houseRepo.SaveChangesAsync();

        return new HouseholdDto
        {
            Id = household.Id,
            Name = household.Name,
            ParentId = household.ParentId,
            MemberCount = 1
        };
    }

    public async Task<HouseholdDto?> UpdateAsync(Guid id, UpdateHouseholdRequest request)
    {
        var household = await _houseRepo.GetByIdAsync(id);
        if (household == null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
            household.Name = request.Name;

        await _houseRepo.UpdateAsync(household);
        await _houseRepo.SaveChangesAsync();

        return new HouseholdDto
        {
            Id = household.Id,
            Name = household.Name,
            ParentId = household.ParentId
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var household = await _houseRepo.GetByIdAsync(id);
        if (household == null) return false;

        await _houseRepo.DeleteAsync(household);
        await _houseRepo.SaveChangesAsync();

        return true;
    }
}