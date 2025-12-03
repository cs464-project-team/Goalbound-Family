using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var result = await _service.CreateAsync(request);
        return Ok(result);
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> CreateBulk(CreateBulkExpensesRequest request)
    {
        var result = await _service.CreateBulkAsync(request);
        return Ok(result);
    }

    [HttpGet("{householdId:guid}/{year:int}/{month:int}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> Get(Guid householdId, int year, int month)
    {
        return Ok(await _service.GetByHouseholdMonthAsync(householdId, year, month));
    }

    [HttpGet("user/{userId:guid}/{year:int}/{month:int}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetByUser(Guid userId, int year, int month)
    {
        return Ok(await _service.GetByUserMonthAsync(userId, year, month));
    }
}