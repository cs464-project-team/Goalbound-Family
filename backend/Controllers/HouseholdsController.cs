using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/households")]
[Authorize]
public class HouseholdsController : ControllerBase
{
    private readonly IHouseholdService _service;

    public HouseholdsController(IHouseholdService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HouseholdDto>> Get(Guid id)
    {
        var result = await _service.GetAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<HouseholdDto>>> GetByUserId(Guid userId)
    {
        var result = await _service.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpGet("{householdId:guid}/members")]
    public async Task<ActionResult<IEnumerable<HouseholdMemberDto>>> GetMembers(Guid householdId)
    {
        var result = await _service.GetMembersAsync(householdId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<HouseholdDto>> Create(CreateHouseholdRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}