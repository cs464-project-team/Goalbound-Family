using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(CreateExpenseRequest request)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == null)
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            var result = await _service.CreateAsync(request, requestingUserId.Value);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> CreateBulk(CreateBulkExpensesRequest request)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == null)
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            var result = await _service.CreateBulkAsync(request, requestingUserId.Value);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{householdId:guid}/{year:int}/{month:int}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> Get(Guid householdId, int year, int month)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == null)
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            return Ok(await _service.GetByHouseholdMonthAsync(householdId, year, month, requestingUserId.Value));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("user/{userId:guid}/{year:int}/{month:int}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetByUser(Guid userId, int year, int month)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == null)
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            return Ok(await _service.GetByUserMonthAsync(userId, year, month, requestingUserId.Value));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    private Guid? GetAuthenticatedUserId()
    {
        // Use ClaimTypes.NameIdentifier instead of "sub"
        // because .NET JWT middleware transforms the "sub" claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}