using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.Models;

public class BudgetCategory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid HouseholdId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public Household? Household { get; set; }
}