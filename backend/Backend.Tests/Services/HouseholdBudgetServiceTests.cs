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

public class HouseholdBudgetServiceTests
{
    private readonly Mock<IHouseholdBudgetRepository> _budgetRepositoryMock;
    private readonly Mock<IBudgetCategoryRepository> _categoryRepositoryMock;
    private readonly HouseholdBudgetService _service;

    public HouseholdBudgetServiceTests()
    {
        _budgetRepositoryMock = new Mock<IHouseholdBudgetRepository>();
        _categoryRepositoryMock = new Mock<IBudgetCategoryRepository>();
        _service = new HouseholdBudgetService(
            _budgetRepositoryMock.Object,
            _categoryRepositoryMock.Object);
    }

    #region GetBudgetsAsync Tests

    [Fact]
    public async Task GetBudgetsAsync_BudgetsExist_ReturnsListOfHouseholdBudgetDtos()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var year = 2025;
        var month = 12;

        var budgets = new List<HouseholdBudget>
        {
            new HouseholdBudget
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = categoryId1,
                Limit = 500m,
                Year = year,
                Month = month,
                Category = new BudgetCategory { Id = categoryId1, Name = "Groceries" }
            },
            new HouseholdBudget
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                CategoryId = categoryId2,
                Limit = 200m,
                Year = year,
                Month = month,
                Category = new BudgetCategory { Id = categoryId2, Name = "Entertainment" }
            }
        };

        _budgetRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(budgets);

        // Act
        var result = await _service.GetBudgetsAsync(householdId, year, month);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.CategoryName == "Groceries" && b.Limit == 500m);
        result.Should().Contain(b => b.CategoryName == "Entertainment" && b.Limit == 200m);
    }

    [Fact]
    public async Task GetBudgetsAsync_NoBudgets_ReturnsEmptyList()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        _budgetRepositoryMock.Setup(x => x.GetByHouseholdMonthAsync(householdId, 2025, 12))
            .ReturnsAsync(new List<HouseholdBudget>());

        // Act
        var result = await _service.GetBudgetsAsync(householdId, 2025, 12);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateOrUpdateAsync Tests

    [Fact]
    public async Task CreateOrUpdateAsync_BudgetDoesNotExist_CreatesNewBudget()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new CreateHouseholdBudgetRequest
        {
            HouseholdId = householdId,
            CategoryId = categoryId,
            Limit = 750m,
            Year = 2025,
            Month = 12
        };

        var category = new BudgetCategory { Id = categoryId, Name = "Utilities" };

        _budgetRepositoryMock.Setup(x => x.GetByHouseholdCategoryMonthAsync(
                householdId, categoryId, 2025, 12))
            .ReturnsAsync((HouseholdBudget?)null);

        _budgetRepositoryMock.Setup(x => x.AddAsync(It.IsAny<HouseholdBudget>()))
            .Returns(Task.FromResult<HouseholdBudget>(null!));

        _budgetRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(0));

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _service.CreateOrUpdateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Limit.Should().Be(750m);
        result.CategoryName.Should().Be("Utilities");

        _budgetRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdBudget>()), Times.Once);
        _budgetRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_BudgetExists_UpdatesExistingBudget()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();

        var existingBudget = new HouseholdBudget
        {
            Id = budgetId,
            HouseholdId = householdId,
            CategoryId = categoryId,
            Limit = 500m,
            Year = 2025,
            Month = 12,
            Category = new BudgetCategory { Id = categoryId, Name = "Food" }
        };

        var request = new CreateHouseholdBudgetRequest
        {
            HouseholdId = householdId,
            CategoryId = categoryId,
            Limit = 800m,
            Year = 2025,
            Month = 12
        };

        _budgetRepositoryMock.Setup(x => x.GetByHouseholdCategoryMonthAsync(
                householdId, categoryId, 2025, 12))
            .ReturnsAsync(existingBudget);

        _budgetRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<HouseholdBudget>()))
            .Returns(Task.FromResult(1));

        _budgetRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.CreateOrUpdateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Limit.Should().Be(800m);
        result.CategoryName.Should().Be("Food");

        _budgetRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<HouseholdBudget>()), Times.Once);
        _budgetRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdBudget>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_BudgetExists_ReturnsTrue()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var budget = new HouseholdBudget
        {
            Id = budgetId,
            HouseholdId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Limit = 300m,
            Year = 2025,
            Month = 12
        };

        _budgetRepositoryMock.Setup(x => x.GetByIdAsync(budgetId))
            .ReturnsAsync(budget);

        _budgetRepositoryMock.Setup(x => x.DeleteAsync(budget))
            .Returns(Task.FromResult(1));

        _budgetRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(0));

        // Act
        var result = await _service.DeleteAsync(budgetId);

        // Assert
        result.Should().BeTrue();
        _budgetRepositoryMock.Verify(x => x.DeleteAsync(budget), Times.Once);
        _budgetRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_BudgetDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        _budgetRepositoryMock.Setup(x => x.GetByIdAsync(budgetId))
            .ReturnsAsync((HouseholdBudget?)null);

        // Act
        var result = await _service.DeleteAsync(budgetId);

        // Assert
        result.Should().BeFalse();
        _budgetRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<HouseholdBudget>()), Times.Never);
        _budgetRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
