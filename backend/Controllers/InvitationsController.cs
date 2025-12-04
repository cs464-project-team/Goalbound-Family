using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/invitations")]
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
        var result = await _service.CreateAsync(request);
        return Ok(result);
    }

    [HttpPost("accept")]
    public async Task<ActionResult> Accept(AcceptInvitationRequest request)
    {
        var result = await _service.AcceptAsync(request);
        return result ? Ok() : BadRequest("Invalid or expired invitation");
    }
}