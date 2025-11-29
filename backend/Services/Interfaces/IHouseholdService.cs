using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IHouseholdService
{
    Task<HouseholdDto?> GetAsync(Guid id);
    Task<IEnumerable<HouseholdDto>> GetAllAsync();
    Task<HouseholdDto> CreateAsync(CreateHouseholdRequest request);
    Task<HouseholdDto?> UpdateAsync(Guid id, UpdateHouseholdRequest request);
    Task<bool> DeleteAsync(Guid id);
}