using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HouseholdMembersController : ControllerBase
{
    private readonly IHouseholdMemberService _service;

    public HouseholdMembersController(IHouseholdMemberService service)
    {
        _service = service;
    }

    [HttpGet("{householdId:guid}")]
    public async Task<ActionResult<IEnumerable<HouseholdMemberDto>>> Get(Guid householdId)
    {
        var members = await _service.GetMembersAsync(householdId);
        return Ok(members);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<HouseholdDto>>> GetHouseholdsForUser(Guid userId)
    {
        var households = await _service.GetHouseholdsForUserAsync(userId);
        return Ok(households);
    }

    // GET api/householdmembers/{householdId}/user/{userId}
    [HttpGet("{householdId:guid}/user/{userId:guid}")]
    public async Task<ActionResult<HouseholdMemberDto?>> GetMemberByUserAndHousehold(Guid householdId, Guid userId)
    {
        var member = await _service.GetByUserAndHouseholdAsync(userId, householdId);

        if (member == null) return NotFound();

        return Ok(member);
    }
}