using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.Models;

public class HouseholdBudget
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid HouseholdId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public decimal Limit { get; set; }

    [Required]
    public int Year { get; set; }

    [Required]
    public int Month { get; set; }

    public BudgetCategory? Category { get; set; }
    public Household? Household { get; set; }
}