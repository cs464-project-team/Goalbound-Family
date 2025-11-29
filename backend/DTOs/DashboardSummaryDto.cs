namespace GoalboundFamily.Api.DTOs;

public class CategorySummaryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetLimit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => BudgetLimit - Spent;
    public decimal Progress => BudgetLimit == 0 ? 0 : Decimal.Round((Spent / BudgetLimit) * 100, 2);
}

public class DashboardSummaryDto
{
    public Guid HouseholdId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public IEnumerable<CategorySummaryDto> Categories { get; set; } = new List<CategorySummaryDto>();
    public decimal TotalBudget => Categories.Sum(c => c.BudgetLimit);
    public decimal TotalSpent => Categories.Sum(c => c.Spent);
}