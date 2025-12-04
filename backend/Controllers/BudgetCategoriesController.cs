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
        return Ok(await _service.GetCategoriesAsync(householdId));
    }

    [HttpPost]
    public async Task<ActionResult<BudgetCategoryDto>> Create(CreateBudgetCategoryRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) ? Ok() : NotFound();
    }
}