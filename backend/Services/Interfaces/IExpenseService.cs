using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IExpenseService
{
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request);
    Task<IEnumerable<ExpenseDto>> CreateBulkAsync(CreateBulkExpensesRequest request);
    Task<IEnumerable<ExpenseDto>> GetByHouseholdMonthAsync(Guid householdId, int year, int month);
    Task<IEnumerable<ExpenseDto>> GetByUserMonthAsync(Guid userId, int year, int month);
}