using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class HouseholdService : IHouseholdService
{
    private readonly IHouseholdRepository _houseRepo;
    private readonly IHouseholdMemberRepository _memberRepo;

    public HouseholdService(
        IHouseholdRepository houseRepo,
        IHouseholdMemberRepository memberRepo)
    {
        _houseRepo = houseRepo;
        _memberRepo = memberRepo;
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