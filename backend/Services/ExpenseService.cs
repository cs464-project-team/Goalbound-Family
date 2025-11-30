using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repo;
    private readonly IBudgetCategoryRepository _categoryRepo;

    public ExpenseService(IExpenseRepository repo, IBudgetCategoryRepository categoryRepo)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request)
    {
        if (request.Amount <= 0) throw new ArgumentException("Amount must be > 0");

        var expense = new Expense
        {
            HouseholdId = request.HouseholdId,
            CreatedByUserId = request.CreatedByUserId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description
        };

        await _repo.AddAsync(expense);
        await _repo.SaveChangesAsync();

        var cat = await _categoryRepo.GetByIdAsync(expense.CategoryId);

        return new ExpenseDto
        {
            Id = expense.Id,
            HouseholdId = expense.HouseholdId,
            CreatedByUserId = expense.CreatedByUserId,
            CategoryId = expense.CategoryId,
            CategoryName = cat?.Name ?? string.Empty,
            Amount = expense.Amount,
            Date = expense.Date,
            Description = expense.Description
        };
    }

    public async Task<IEnumerable<ExpenseDto>> GetByHouseholdMonthAsync(Guid householdId, int year, int month)
    {
        var expenses = await _repo.GetByHouseholdMonthAsync(householdId, year, month);
        return expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            HouseholdId = e.HouseholdId,
            CreatedByUserId = e.CreatedByUserId,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name ?? string.Empty,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description
        });
    }
}