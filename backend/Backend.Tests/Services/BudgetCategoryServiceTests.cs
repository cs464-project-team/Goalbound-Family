using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services;
using GoalboundFamily.Api.Services.Interfaces;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class BudgetCategoryServiceTests
{
    private readonly Mock<IBudgetCategoryRepository> _repositoryMock;
    private readonly Mock<IHouseholdAuthorizationService> _authServiceMock;
    private readonly BudgetCategoryService _service;

    public BudgetCategoryServiceTests()
    {
        _repositoryMock = new Mock<IBudgetCategoryRepository>();
        _authServiceMock = new Mock<IHouseholdAuthorizationService>();

        // Setup auth service to allow all validations by default
        _authServiceMock
            .Setup(a => a.ValidateHouseholdAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _service = new BudgetCategoryService(_repositoryMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task GetCategoriesAsync_ValidHousehold_ReturnsCategories()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();

        var categories = new List<BudgetCategory>
        {
            new() { Id = Guid.NewGuid(), HouseholdId = householdId, Name = "Groceries" },
            new() { Id = Guid.NewGuid(), HouseholdId = householdId, Name = "Utilities" }
        };

        _repositoryMock
            .Setup(r => r.GetByHouseholdAsync(householdId))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetCategoriesAsync(householdId, requestingUserId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Groceries");
        result.Should().Contain(c => c.Name == "Utilities");

        _authServiceMock.Verify(a => a.ValidateHouseholdAccessAsync(requestingUserId, householdId), Times.Once);
        _repositoryMock.Verify(r => r.GetByHouseholdAsync(householdId), Times.Once);
    }

    [Fact]
    public async Task GetCategoriesAsync_NoCategories_ReturnsEmptyList()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByHouseholdAsync(householdId))
            .ReturnsAsync(new List<BudgetCategory>());

        // Act
        var result = await _service.GetCategoriesAsync(householdId, requestingUserId);

        // Assert
        result.Should().BeEmpty();

        _authServiceMock.Verify(a => a.ValidateHouseholdAccessAsync(requestingUserId, householdId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidCategory_ReturnsCreatedCategory()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();

        var request = new CreateBudgetCategoryRequest
        {
            HouseholdId = householdId,
            Name = "Entertainment"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<BudgetCategory>()))
            .ReturnsAsync((BudgetCategory c) => c);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(request, requestingUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Entertainment");

        _authServiceMock.Verify(a => a.ValidateHouseholdAccessAsync(requestingUserId, householdId), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<BudgetCategory>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryExists_ReturnsTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();

        var category = new BudgetCategory
        {
            Id = categoryId,
            HouseholdId = householdId,
            Name = "Transport"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(r => r.DeleteAsync(category))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteAsync(categoryId, requestingUserId);

        // Assert
        result.Should().BeTrue();

        _authServiceMock.Verify(a => a.ValidateHouseholdAccessAsync(requestingUserId, householdId), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(category), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((BudgetCategory?)null);

        // Act
        var result = await _service.DeleteAsync(categoryId, requestingUserId);

        // Assert
        result.Should().BeFalse();

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<BudgetCategory>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
