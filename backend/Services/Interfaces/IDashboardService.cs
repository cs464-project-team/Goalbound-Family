using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetHouseholdMonthlySummaryAsync(Guid householdId, int year, int month);
}