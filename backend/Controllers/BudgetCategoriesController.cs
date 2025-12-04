using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/budgets/categories")]
[Authorize]
public class BudgetCategoriesController : ControllerBase
{
    private readonly IBudgetCategoryService _service;

    public BudgetCategoriesController(IBudgetCategoryService service)
    {
        _service = service;
    }

    [HttpGet("{householdId:guid}")]
    public async Task<ActionResult<IEnumerable<BudgetCategoryDto>>> Get(Guid householdId)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == null)
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            return Ok(await _service.GetCategoriesAsync(householdId, requestingUserId.Value));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<BudgetCategoryDto>> Create(CreateBudgetCategoryRequest request)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == null)
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            var created = await _service.CreateAsync(request, requestingUserId.Value);
            return Ok(created);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == null)
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            return await _service.DeleteAsync(id, requestingUserId.Value) ? Ok() : NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    private Guid? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}