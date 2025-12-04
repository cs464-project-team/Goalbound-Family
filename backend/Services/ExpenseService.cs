using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repo;
    private readonly IBudgetCategoryRepository _categoryRepo;
    private readonly IHouseholdAuthorizationService _authService;
    private readonly IQuestProgressService _questService;


    public ExpenseService(
        IExpenseRepository repo,
        IBudgetCategoryRepository categoryRepo,
        IHouseholdAuthorizationService authService, IQuestProgressService questService)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
        _questService = questService;
        _authService = authService;
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, Guid requestingUserId)
    {
        if (request.Amount <= 0) throw new ArgumentException("Amount must be > 0");

        // Verify requesting user is in the household
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, request.HouseholdId);

        // Verify the target user (if different) is also in the household
        if (request.UserId != requestingUserId)
        {
            await _authService.ValidateHouseholdAccessAsync(request.UserId, request.HouseholdId);
        }

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

        // **Trigger quest progress event**
        await _questService.HandleExpenseLogged(expense.UserId, expense.HouseholdId, cat?.Name ?? string.Empty);

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

    public async Task<IEnumerable<ExpenseDto>> CreateBulkAsync(CreateBulkExpensesRequest request, Guid requestingUserId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("At least one expense item is required");

        // Verify requesting user is in the household
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, request.HouseholdId);

        var expenses = new List<Expense>();

        foreach (var item in request.Items)
        {
            if (item.Amount <= 0)
                throw new ArgumentException($"Amount must be > 0 for all items");

            // Verify each target user is in the household
            await _authService.ValidateHouseholdAccessAsync(item.UserId, request.HouseholdId);

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

        // **Trigger quest progress events for each user**
        foreach (var expense in expenses)
        {
            await _questService.HandleExpenseLogged(expense.UserId, expense.HouseholdId, cat?.Name ?? string.Empty);
        }

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

    public async Task<IEnumerable<ExpenseDto>> GetByHouseholdMonthAsync(Guid householdId, int year, int month, Guid requestingUserId)
    {
        // Verify requesting user is in the household
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, householdId);

        var expenses = await _repo.GetByHouseholdMonthAsync(householdId, year, month);
        return expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            HouseholdId = e.HouseholdId,
            HouseholdName = e.Household?.Name ?? string.Empty,
            UserId = e.UserId,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name ?? string.Empty,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description,
            ReceiptId = e.ReceiptId
        });
    }

    public async Task<IEnumerable<ExpenseDto>> GetByUserMonthAsync(Guid userId, int year, int month, Guid requestingUserId)
    {
        // Only allow users to query their own expenses OR require household membership validation
        if (userId != requestingUserId)
        {
            throw new UnauthorizedAccessException("Users can only query their own expenses");
        }

        // Get user's current household IDs
        var userHouseholdIds = await _authService.GetUserHouseholdIdsAsync(requestingUserId);

        // Filter expenses to only include those from households the user is currently in
        var expenses = await _repo.GetByUserMonthFilteredAsync(userId, year, month, userHouseholdIds);
        return expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            HouseholdId = e.HouseholdId,
            HouseholdName = e.Household?.Name ?? string.Empty,
            UserId = e.UserId,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name ?? string.Empty,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description,
            ReceiptId = e.ReceiptId
        });
    }
}