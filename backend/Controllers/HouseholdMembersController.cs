using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/householdmembers")]
[Authorize]
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

    [HttpDelete("{memberId:guid}")]
    public async Task<ActionResult> Delete(Guid memberId, [FromQuery] Guid requestingUserId)
    {
        if (requestingUserId == Guid.Empty)
        {
            return BadRequest(new { message = "Requesting user ID is required" });
        }

        var success = await _service.RemoveMemberAsync(memberId, requestingUserId);
        if (!success)
        {
            return NotFound(new { message = "Member not found or unauthorized" });
        }

        return NoContent();
    }
}