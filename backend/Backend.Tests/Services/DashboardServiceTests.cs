using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IHouseholdBudgetRepository> _budgetRepositoryMock;
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock;
    private readonly Mock<IBudgetCategoryRepository> _categoryRepositoryMock;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _budgetRepositoryMock = new Mock<IHouseholdBudgetRepository>();
        _expenseRepositoryMock = new Mock<IExpenseRepository>();
        _categoryRepositoryMock = new Mock<IBudgetCategoryRepository>();
        _service = new DashboardService(
            _budgetRepositoryMock.Object,
            _expenseRepositoryMock.Object,
            _categoryRepositoryMock.Object);
    }

    #region GetHouseholdMonthlySummaryAsync Tests

    [Fact]
    public async Task GetHouseholdMonthlySummaryAsync_WithBudgetsAndExpenses_ReturnsSummary()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var groceryCategoryId = Guid.NewGuid();
        var transportCategoryId = Guid.NewGuid();
        var year = 2025;
        var month = 12;

        var budgets = new List<HouseholdBudget>
        {
            new HouseholdBudget
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = groceryCategoryId,
                Limit = 500m,
                Year = year,
                Month = month
            },
            new HouseholdBudget
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = transportCategoryId,
                Limit = 300m,
                Year = year,
                Month = month
            }
        };

        var expenses = new List<Expense>
        {
            new Expense
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = groceryCategoryId,
                Amount = 350m,
                Date = new DateTime(year, month, 15)
            },
            new Expense
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = transportCategoryId,
                Amount = 150m,
                Date = new DateTime(year, month, 20)
            }
        };

        var categories = new List<BudgetCategory>
        {
            new BudgetCategory { Id = groceryCategoryId, Name = "Groceries" },
            new BudgetCategory { Id = transportCategoryId, Name = "Transportation" }
        };

        _budgetRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(budgets);

        _expenseRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(expenses);

        _categoryRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetHouseholdMonthlySummaryAsync(householdId, year, month);

        // Assert
        result.Should().NotBeNull();
        result.HouseholdId.Should().Be(householdId);
        result.Year.Should().Be(year);
        result.Month.Should().Be(month);
        result.Categories.Should().HaveCount(2);

        var grocerySummary = result.Categories.First(c => c.CategoryId == groceryCategoryId);
        grocerySummary.CategoryName.Should().Be("Groceries");
        grocerySummary.BudgetLimit.Should().Be(500m);
        grocerySummary.Spent.Should().Be(350m);

        var transportSummary = result.Categories.First(c => c.CategoryId == transportCategoryId);
        transportSummary.CategoryName.Should().Be("Transportation");
        transportSummary.BudgetLimit.Should().Be(300m);
        transportSummary.Spent.Should().Be(150m);
    }

    [Fact]
    public async Task GetHouseholdMonthlySummaryAsync_WithExpensesButNoBudget_IncludesUnbudgetedCategories()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var budgetedCategoryId = Guid.NewGuid();
        var unbudgetedCategoryId = Guid.NewGuid();
        var year = 2025;
        var month = 12;

        var budgets = new List<HouseholdBudget>
        {
            new HouseholdBudget
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = budgetedCategoryId,
                Limit = 500m,
                Year = year,
                Month = month
            }
        };

        var expenses = new List<Expense>
        {
            new Expense
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = budgetedCategoryId,
                Amount = 300m,
                Date = new DateTime(year, month, 10)
            },
            new Expense
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = unbudgetedCategoryId,
                Amount = 100m,
                Date = new DateTime(year, month, 15)
            }
        };

        var categories = new List<BudgetCategory>
        {
            new BudgetCategory { Id = budgetedCategoryId, Name = "Food" },
            new BudgetCategory { Id = unbudgetedCategoryId, Name = "Miscellaneous" }
        };

        _budgetRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(budgets);

        _expenseRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(expenses);

        _categoryRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetHouseholdMonthlySummaryAsync(householdId, year, month);

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().HaveCount(2);

        var budgetedCategory = result.Categories.First(c => c.CategoryId == budgetedCategoryId);
        budgetedCategory.BudgetLimit.Should().Be(500m);
        budgetedCategory.Spent.Should().Be(300m);

        var unbudgetedCategory = result.Categories.First(c => c.CategoryId == unbudgetedCategoryId);
        unbudgetedCategory.BudgetLimit.Should().Be(0m);
        unbudgetedCategory.Spent.Should().Be(100m);
    }

    [Fact]
    public async Task GetHouseholdMonthlySummaryAsync_WithBudgetButNoExpenses_ShowsZeroSpent()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var year = 2025;
        var month = 12;

        var budgets = new List<HouseholdBudget>
        {
            new HouseholdBudget
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = categoryId,
                Limit = 1000m,
                Year = year,
                Month = month
            }
        };

        var categories = new List<BudgetCategory>
        {
            new BudgetCategory { Id = categoryId, Name = "Savings" }
        };

        _budgetRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(budgets);

        _expenseRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(new List<Expense>());

        _categoryRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetHouseholdMonthlySummaryAsync(householdId, year, month);

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().HaveCount(1);

        var categorySummary = result.Categories.First();
        categorySummary.BudgetLimit.Should().Be(1000m);
        categorySummary.Spent.Should().Be(0m);
    }

    [Fact]
    public async Task GetHouseholdMonthlySummaryAsync_NoBudgetsAndNoExpenses_ReturnsEmptySummary()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var year = 2025;
        var month = 12;

        _budgetRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(new List<HouseholdBudget>());

        _expenseRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(new List<Expense>());

        _categoryRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<BudgetCategory>());

        // Act
        var result = await _service.GetHouseholdMonthlySummaryAsync(householdId, year, month);

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().BeEmpty();
    }

    #endregion
}
