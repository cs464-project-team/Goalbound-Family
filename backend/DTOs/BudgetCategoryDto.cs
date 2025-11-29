namespace GoalboundFamily.Api.DTOs;

public class BudgetCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


public class CreateBudgetCategoryRequest
{
    public Guid HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
}