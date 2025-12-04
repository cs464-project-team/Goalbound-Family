using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class HouseholdServiceTests
{
    private readonly Mock<IHouseholdRepository> _householdRepositoryMock;
    private readonly Mock<IHouseholdMemberRepository> _memberRepositoryMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly HouseholdService _service;

    public HouseholdServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _householdRepositoryMock = new Mock<IHouseholdRepository>();
        _memberRepositoryMock = new Mock<IHouseholdMemberRepository>();
        _service = new HouseholdService(
            _householdRepositoryMock.Object,
            _memberRepositoryMock.Object,
            _dbContext);
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_HouseholdExists_ReturnsHouseholdDto()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var household = new Household
        {
            Id = householdId,
            Name = "Smith Family",
            ParentId = parentId,
            Members = new List<HouseholdMember>
            {
                new HouseholdMember { Id = Guid.NewGuid(), UserId = parentId, Role = "Parent" },
                new HouseholdMember { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Role = "Member" }
            }
        };

        _householdRepositoryMock.Setup(x => x.GetWithMembersAsync(householdId))
            .ReturnsAsync(household);

        // Act
        var result = await _service.GetAsync(householdId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(householdId);
        result.Name.Should().Be("Smith Family");
        result.ParentId.Should().Be(parentId);
        result.MemberCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAsync_HouseholdDoesNotExist_ReturnsNull()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        _householdRepositoryMock.Setup(x => x.GetWithMembersAsync(householdId))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _service.GetAsync(householdId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_HouseholdsExist_ReturnsListOfHouseholdDtos()
    {
        // Arrange
        var households = new List<Household>
        {
            new Household { Id = Guid.NewGuid(), Name = "Family 1", ParentId = Guid.NewGuid() },
            new Household { Id = Guid.NewGuid(), Name = "Family 2", ParentId = Guid.NewGuid() },
            new Household { Id = Guid.NewGuid(), Name = "Family 3", ParentId = Guid.NewGuid() }
        };

        _householdRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(households);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(h => h.Name).Should().Contain(new[] { "Family 1", "Family 2", "Family 3" });
    }

    [Fact]
    public async Task GetAllAsync_NoHouseholds_ReturnsEmptyList()
    {
        // Arrange
        _householdRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Household>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsHouseholdDto()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new CreateHouseholdRequest
        {
            Name = "New Family",
            ParentId = parentId
        };

        _householdRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Household>()))
            .ReturnsAsync(new Household { Id = Guid.NewGuid(), Name = request.Name, ParentId = request.ParentId });

        _memberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<HouseholdMember>()))
            .Returns(Task.FromResult(new HouseholdMember()));

        _householdRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        _memberRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Family");
        result.ParentId.Should().Be(parentId);
        result.MemberCount.Should().Be(1);

        _householdRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Household>()), Times.Once);
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdMember>()), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedHouseholdDto()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var household = new Household
        {
            Id = householdId,
            Name = "Old Name",
            ParentId = Guid.NewGuid()
        };

        var request = new UpdateHouseholdRequest
        {
            Name = "Updated Name"
        };

        _householdRepositoryMock.Setup(x => x.GetByIdAsync(householdId))
            .ReturnsAsync(household);

        _householdRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Household>()))
            .Returns(Task.FromResult(1));

        _householdRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.UpdateAsync(householdId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");

        _householdRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Household>()), Times.Once);
        _householdRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_HouseholdDoesNotExist_ReturnsNull()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var request = new UpdateHouseholdRequest
        {
            Name = "New Name"
        };

        _householdRepositoryMock.Setup(x => x.GetByIdAsync(householdId))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _service.UpdateAsync(householdId, request);

        // Assert
        result.Should().BeNull();
        _householdRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Household>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_HouseholdExists_ReturnsTrue()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var household = new Household
        {
            Id = householdId,
            Name = "To Delete",
            ParentId = Guid.NewGuid()
        };

        _householdRepositoryMock.Setup(x => x.GetByIdAsync(householdId))
            .ReturnsAsync(household);

        _householdRepositoryMock.Setup(x => x.DeleteAsync(household))
            .Returns(Task.FromResult(1));

        _householdRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.DeleteAsync(householdId);

        // Assert
        result.Should().BeTrue();
        _householdRepositoryMock.Verify(x => x.DeleteAsync(household), Times.Once);
        _householdRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_HouseholdDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        _householdRepositoryMock.Setup(x => x.GetByIdAsync(householdId))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _service.DeleteAsync(householdId);

        // Assert
        result.Should().BeFalse();
        _householdRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Household>()), Times.Never);
    }

    #endregion
}
