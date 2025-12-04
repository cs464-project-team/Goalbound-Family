using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/householdbudgets")]
public class HouseholdBudgetsController : ControllerBase
{
    private readonly IHouseholdBudgetService _service;

    public HouseholdBudgetsController(IHouseholdBudgetService service)
    {
        _service = service;
    }

    [HttpGet("{householdId:guid}/{year:int}/{month:int}")]
    public async Task<ActionResult<IEnumerable<HouseholdBudgetDto>>> Get(Guid householdId, int year, int month)
    {
        return Ok(await _service.GetBudgetsAsync(householdId, year, month));
    }

    [HttpPost]
    public async Task<ActionResult<HouseholdBudgetDto>> CreateOrUpdate(CreateHouseholdBudgetRequest request)
    {
        var result = await _service.CreateOrUpdateAsync(request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) ? Ok() : NotFound();
    }
}