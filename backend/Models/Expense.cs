using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.Models;

public class Expense
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid HouseholdId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public BudgetCategory? Category { get; set; }
    public Household? Household { get; set; }
    public User? CreatedByUser { get; set; }
}