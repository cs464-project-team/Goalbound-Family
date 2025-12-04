using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IExpenseService
{
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, Guid requestingUserId);
    Task<IEnumerable<ExpenseDto>> CreateBulkAsync(CreateBulkExpensesRequest request, Guid requestingUserId);
    Task<IEnumerable<ExpenseDto>> GetByHouseholdMonthAsync(Guid householdId, int year, int month, Guid requestingUserId);
    Task<IEnumerable<ExpenseDto>> GetByUserMonthAsync(Guid userId, int year, int month, Guid requestingUserId);
}