using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService _service;

    public InvitationsController(IInvitationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<InvitationDto>> Create(CreateInvitationRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<ActionResult> Accept(AcceptInvitationRequest request)
    {
        var result = await _service.AcceptAsync(request);
        return result ? Ok() : BadRequest("Invalid or expired invitation");
    }
}