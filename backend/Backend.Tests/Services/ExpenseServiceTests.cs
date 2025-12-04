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

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock;
    private readonly Mock<IBudgetCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IHouseholdAuthorizationService> _authServiceMock;
    private readonly Mock<IQuestProgressService> _questProgressServiceMock;
    private readonly ExpenseService _service;

    public ExpenseServiceTests()
    {
        _expenseRepositoryMock = new Mock<IExpenseRepository>();
        _categoryRepositoryMock = new Mock<IBudgetCategoryRepository>();
        _authServiceMock = new Mock<IHouseholdAuthorizationService>();
        _questProgressServiceMock = new Mock<IQuestProgressService>();

        // Setup auth service to allow all validations by default
        _authServiceMock
            .Setup(a => a.ValidateHouseholdAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(a => a.GetUserHouseholdIdsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Guid>());

        _service = new ExpenseService(_expenseRepositoryMock.Object, _categoryRepositoryMock.Object, _authServiceMock.Object, _questProgressServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidExpense_ReturnsExpenseDto()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var request = new CreateExpenseRequest
        {
            HouseholdId = householdId,
            UserId = userId,
            CategoryId = categoryId,
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            Description = "Grocery shopping"
        };

        var category = new BudgetCategory
        {
            Id = categoryId,
            Name = "Groceries"
        };

        _expenseRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync((Expense e) => e);

        _expenseRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _service.CreateAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.HouseholdId.Should().Be(householdId);
        result.UserId.Should().Be(userId);
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be("Groceries");
        result.Amount.Should().Be(50.00m);
        result.Description.Should().Be("Grocery shopping");

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
        _expenseRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            HouseholdId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Amount = 0m,
            Date = DateTime.UtcNow,
            Description = "Invalid expense"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.CreateAsync(request, Guid.NewGuid());
        });

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            HouseholdId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Amount = -10.00m,
            Date = DateTime.UtcNow,
            Description = "Invalid expense"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.CreateAsync(request, Guid.NewGuid());
        });

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
    }

    [Fact]
    public async Task CreateBulkAsync_ValidExpenses_ReturnsExpenseDtoList()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        var request = new CreateBulkExpensesRequest
        {
            HouseholdId = householdId,
            CategoryId = categoryId,
            Date = DateTime.UtcNow,
            Items = new List<BulkExpenseItem>
            {
                new() { UserId = user1Id, Amount = 25.00m, Description = "User 1 expense" },
                new() { UserId = user2Id, Amount = 35.00m, Description = "User 2 expense" }
            }
        };

        var category = new BudgetCategory
        {
            Id = categoryId,
            Name = "Dining"
        };

        _expenseRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync((Expense e) => e);

        _expenseRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _service.CreateBulkAsync(request, user1Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.HouseholdId == householdId);
        result.Should().OnlyContain(e => e.CategoryId == categoryId);
        result.Should().OnlyContain(e => e.CategoryName == "Dining");

        var firstExpense = result.First();
        firstExpense.UserId.Should().Be(user1Id);
        firstExpense.Amount.Should().Be(25.00m);

        var secondExpense = result.Skip(1).First();
        secondExpense.UserId.Should().Be(user2Id);
        secondExpense.Amount.Should().Be(35.00m);

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Exactly(2));
        _expenseRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBulkAsync_EmptyItemsList_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateBulkExpensesRequest
        {
            HouseholdId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Items = new List<BulkExpenseItem>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.CreateBulkAsync(request, Guid.NewGuid());
        });

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
    }

    [Fact]
    public async Task CreateBulkAsync_NullItemsList_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateBulkExpensesRequest
        {
            HouseholdId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Items = null!
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.CreateBulkAsync(request, Guid.NewGuid());
        });

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
    }

    [Fact]
    public async Task CreateBulkAsync_ItemWithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateBulkExpensesRequest
        {
            HouseholdId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Items = new List<BulkExpenseItem>
            {
                new() { UserId = Guid.NewGuid(), Amount = 25.00m, Description = "Valid" },
                new() { UserId = Guid.NewGuid(), Amount = 0m, Description = "Invalid" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.CreateBulkAsync(request, Guid.NewGuid());
        });

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
    }

    [Fact]
    public async Task CreateBulkAsync_ItemWithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateBulkExpensesRequest
        {
            HouseholdId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Items = new List<BulkExpenseItem>
            {
                new() { UserId = Guid.NewGuid(), Amount = 25.00m, Description = "Valid" },
                new() { UserId = Guid.NewGuid(), Amount = -10.00m, Description = "Invalid" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _service.CreateBulkAsync(request, Guid.NewGuid());
        });

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
    }

    [Fact]
    public async Task GetByHouseholdMonthAsync_ValidRequest_ReturnsExpenses()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var year = 2024;
        var month = 1;

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                UserId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Amount = 50.00m,
                Date = new DateTime(2024, 1, 15),
                Description = "Expense 1",
                Household = new Household { Name = "Test Household" },
                Category = new BudgetCategory { Name = "Groceries" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                UserId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Amount = 75.00m,
                Date = new DateTime(2024, 1, 20),
                Description = "Expense 2",
                Household = new Household { Name = "Test Household" },
                Category = new BudgetCategory { Name = "Dining" }
            }
        };

        _expenseRepositoryMock
            .Setup(r => r.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(expenses);

        // Act
        var requestingUserId = Guid.NewGuid();
        var result = await _service.GetByHouseholdMonthAsync(householdId, year, month, requestingUserId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.HouseholdId == householdId);
        result.Should().OnlyContain(e => e.HouseholdName == "Test Household");

        var resultList = result.ToList();
        resultList[0].CategoryName.Should().Be("Groceries");
        resultList[1].CategoryName.Should().Be("Dining");

        _expenseRepositoryMock.Verify(r => r.GetByHouseholdMonthAsync(householdId, year, month), Times.Once);
    }

    [Fact]
    public async Task GetByHouseholdMonthAsync_NoExpenses_ReturnsEmptyList()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var year = 2024;
        var month = 1;

        _expenseRepositoryMock
            .Setup(r => r.GetByHouseholdMonthAsync(householdId, year, month))
            .ReturnsAsync(new List<Expense>());

        // Act
        var result = await _service.GetByHouseholdMonthAsync(householdId, year, month, Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();

        _expenseRepositoryMock.Verify(r => r.GetByHouseholdMonthAsync(householdId, year, month), Times.Once);
    }

    [Fact]
    public async Task GetByUserMonthAsync_ValidRequest_ReturnsUserExpenses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var householdId1 = Guid.NewGuid();
        var householdId2 = Guid.NewGuid();
        var year = 2024;
        var month = 1;

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId1,
                UserId = userId,
                CategoryId = Guid.NewGuid(),
                Amount = 30.00m,
                Date = new DateTime(2024, 1, 10),
                Description = "User expense 1",
                Household = new Household { Name = "Household 1" },
                Category = new BudgetCategory { Name = "Transport" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId2,
                UserId = userId,
                CategoryId = Guid.NewGuid(),
                Amount = 45.00m,
                Date = new DateTime(2024, 1, 25),
                Description = "User expense 2",
                Household = new Household { Name = "Household 2" },
                Category = new BudgetCategory { Name = "Entertainment" }
            }
        };

        var householdIds = new List<Guid> { householdId1, householdId2 };

        _authServiceMock
            .Setup(a => a.GetUserHouseholdIdsAsync(userId))
            .ReturnsAsync(householdIds);

        _expenseRepositoryMock
            .Setup(r => r.GetByUserMonthFilteredAsync(userId, year, month, householdIds))
            .ReturnsAsync(expenses);

        // Act
        var result = await _service.GetByUserMonthAsync(userId, year, month, userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.UserId == userId);

        var resultList = result.ToList();
        resultList[0].Amount.Should().Be(30.00m);
        resultList[0].CategoryName.Should().Be("Transport");
        resultList[1].Amount.Should().Be(45.00m);
        resultList[1].CategoryName.Should().Be("Entertainment");

        _expenseRepositoryMock.Verify(r => r.GetByUserMonthFilteredAsync(userId, year, month, householdIds), Times.Once);
    }

    [Fact]
    public async Task GetByUserMonthAsync_NoExpenses_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        var month = 1;
        var householdIds = new List<Guid> { Guid.NewGuid() };

        _authServiceMock
            .Setup(a => a.GetUserHouseholdIdsAsync(userId))
            .ReturnsAsync(householdIds);

        _expenseRepositoryMock
            .Setup(r => r.GetByUserMonthFilteredAsync(userId, year, month, householdIds))
            .ReturnsAsync(new List<Expense>());

        // Act
        var result = await _service.GetByUserMonthAsync(userId, year, month, userId);

        // Assert
        result.Should().BeEmpty();

        _expenseRepositoryMock.Verify(r => r.GetByUserMonthFilteredAsync(userId, year, month, householdIds), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CategoryNotFound_ReturnsExpenseWithEmptyCategoryName()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            HouseholdId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            Description = "Test expense"
        };

        _expenseRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync((Expense e) => e);

        _expenseRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((BudgetCategory?)null);

        // Act
        var result = await _service.CreateAsync(request, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateBulkAsync_SingleItem_CreatesSuccessfully()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new CreateBulkExpensesRequest
        {
            HouseholdId = householdId,
            CategoryId = categoryId,
            Date = DateTime.UtcNow,
            Items = new List<BulkExpenseItem>
            {
                new() { UserId = userId, Amount = 100.00m, Description = "Single expense" }
            }
        };

        var category = new BudgetCategory
        {
            Id = categoryId,
            Name = "Utilities"
        };

        _expenseRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync((Expense e) => e);

        _expenseRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _service.CreateBulkAsync(request, userId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Amount.Should().Be(100.00m);
        result.First().CategoryName.Should().Be("Utilities");

        _expenseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
    }
}
