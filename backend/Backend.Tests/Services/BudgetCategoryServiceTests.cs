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

public class BudgetCategoryServiceTests
{
    private readonly Mock<IBudgetCategoryRepository> _repositoryMock;
    private readonly BudgetCategoryService _service;

    public BudgetCategoryServiceTests()
    {
        _repositoryMock = new Mock<IBudgetCategoryRepository>();
        _service = new BudgetCategoryService(_repositoryMock.Object);
    }

    #region GetCategoriesAsync Tests

    [Fact]
    public async Task GetCategoriesAsync_CategoriesExist_ReturnsListOfBudgetCategoryDtos()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var categories = new List<BudgetCategory>
        {
            new BudgetCategory { Id = Guid.NewGuid(), HouseholdId = householdId, Name = "Groceries" },
            new BudgetCategory { Id = Guid.NewGuid(), HouseholdId = householdId, Name = "Transportation" },
            new BudgetCategory { Id = Guid.NewGuid(), HouseholdId = householdId, Name = "Entertainment" }
        };

        _repositoryMock.Setup(x => x.GetByHouseholdAsync(householdId))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetCategoriesAsync(householdId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(c => c.Name).Should().Contain(new[] { "Groceries", "Transportation", "Entertainment" });
    }

    [Fact]
    public async Task GetCategoriesAsync_NoCategories_ReturnsEmptyList()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByHouseholdAsync(householdId))
            .ReturnsAsync(new List<BudgetCategory>());

        // Act
        var result = await _service.GetCategoriesAsync(householdId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsBudgetCategoryDto()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var request = new CreateBudgetCategoryRequest
        {
            HouseholdId = householdId,
            Name = "New Category"
        };

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<BudgetCategory>()))
            .ReturnsAsync(new BudgetCategory
            {
                Id = Guid.NewGuid(),
                HouseholdId = request.HouseholdId,
                Name = request.Name
            });

        _repositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Category");

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<BudgetCategory>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_CategoryExists_ReturnsTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new BudgetCategory
        {
            Id = categoryId,
            HouseholdId = Guid.NewGuid(),
            Name = "To Delete"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _repositoryMock.Setup(x => x.DeleteAsync(category))
            .Returns(Task.FromResult(1));

        _repositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(category), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync((BudgetCategory?)null);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<BudgetCategory>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
