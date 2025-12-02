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
            UserId = request.UserId,
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
            UserId = expense.UserId,
            CategoryId = expense.CategoryId,
            CategoryName = cat?.Name ?? string.Empty,
            Amount = expense.Amount,
            Date = expense.Date,
            Description = expense.Description
        };
    }

    public async Task<IEnumerable<ExpenseDto>> CreateBulkAsync(CreateBulkExpensesRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("At least one expense item is required");

        var expenses = new List<Expense>();

        foreach (var item in request.Items)
        {
            if (item.Amount <= 0)
                throw new ArgumentException($"Amount must be > 0 for all items");

            expenses.Add(new Expense
            {
                HouseholdId = request.HouseholdId,
                UserId = item.UserId,
                CategoryId = request.CategoryId,
                Amount = item.Amount,
                Date = request.Date,
                Description = item.Description
            });
        }

        foreach (var expense in expenses)
        {
            await _repo.AddAsync(expense);
        }

        await _repo.SaveChangesAsync();

        var cat = await _categoryRepo.GetByIdAsync(request.CategoryId);
        var categoryName = cat?.Name ?? string.Empty;

        return expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            HouseholdId = e.HouseholdId,
            UserId = e.UserId,
            CategoryId = e.CategoryId,
            CategoryName = categoryName,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description
        }).ToList();
    }

    public async Task<IEnumerable<ExpenseDto>> GetByHouseholdMonthAsync(Guid householdId, int year, int month)
    {
        var expenses = await _repo.GetByHouseholdMonthAsync(householdId, year, month);
        return expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            HouseholdId = e.HouseholdId,
            UserId = e.UserId,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name ?? string.Empty,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description
        });
    }
}