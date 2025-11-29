using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPost]
    public async Task<ActionResult<HouseholdDto>> Create(CreateHouseholdRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}