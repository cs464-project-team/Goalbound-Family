using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("{householdId:guid}/{year:int}/{month:int}")]
    public async Task<ActionResult<DashboardSummaryDto>> Get(Guid householdId, int year, int month)
    {
        return Ok(await _service.GetHouseholdMonthlySummaryAsync(householdId, year, month));
    }
}