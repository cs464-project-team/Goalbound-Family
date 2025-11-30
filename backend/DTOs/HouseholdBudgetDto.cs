namespace GoalboundFamily.Api.DTOs;

public class HouseholdBudgetDto
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}


public class CreateHouseholdBudgetRequest
{
    public Guid HouseholdId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Limit { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}