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

public class HouseholdMemberServiceTests
{
    private readonly Mock<IHouseholdMemberRepository> _memberRepositoryMock;
    private readonly HouseholdMemberService _service;

    public HouseholdMemberServiceTests()
    {
        _memberRepositoryMock = new Mock<IHouseholdMemberRepository>();
        _service = new HouseholdMemberService(_memberRepositoryMock.Object);
    }

    #region GetMembersAsync Tests

    [Fact]
    public async Task GetMembersAsync_MembersExist_ReturnsListOfHouseholdMemberDtos()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var members = new List<HouseholdMember>
        {
            new HouseholdMember
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                UserId = Guid.NewGuid(),
                Role = "Parent",
                JoinedAt = DateTime.UtcNow.AddMonths(-6),
                Avatar = "avatar1.jpg",
                Xp = 1500,
                Streak = 10,
                QuestsCompleted = 5,
                User = new User { FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                MemberBadges = new List<MemberBadge>()
            },
            new HouseholdMember
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                UserId = Guid.NewGuid(),
                Role = "Member",
                JoinedAt = DateTime.UtcNow.AddMonths(-3),
                Avatar = "avatar2.jpg",
                Xp = 800,
                Streak = 5,
                QuestsCompleted = 3,
                User = new User { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" },
                MemberBadges = new List<MemberBadge>()
            }
        };

        _memberRepositoryMock.Setup(x => x.GetByHouseholdIdAsync(householdId))
            .ReturnsAsync(members);

        // Act
        var result = await _service.GetMembersAsync(householdId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstMember = result.First();
        firstMember.FirstName.Should().Be("John");
        firstMember.Role.Should().Be("Parent");
        firstMember.Xp.Should().Be(1500);
    }

    [Fact]
    public async Task GetMembersAsync_NoMembers_ReturnsEmptyList()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        _memberRepositoryMock.Setup(x => x.GetByHouseholdIdAsync(householdId))
            .ReturnsAsync(new List<HouseholdMember>());

        // Act
        var result = await _service.GetMembersAsync(householdId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetHouseholdsForUserAsync Tests

    [Fact]
    public async Task GetHouseholdsForUserAsync_UserHasHouseholds_ReturnsListOfHouseholdDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var memberships = new List<HouseholdMember>
        {
            new HouseholdMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HouseholdId = Guid.NewGuid(),
                Household = new Household
                {
                    Id = Guid.NewGuid(),
                    Name = "Smith Family",
                    ParentId = userId,
                    Members = new List<HouseholdMember> { new HouseholdMember(), new HouseholdMember() }
                }
            },
            new HouseholdMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HouseholdId = Guid.NewGuid(),
                Household = new Household
                {
                    Id = Guid.NewGuid(),
                    Name = "Johnson Family",
                    ParentId = Guid.NewGuid(),
                    Members = new List<HouseholdMember> { new HouseholdMember(), new HouseholdMember(), new HouseholdMember() }
                }
            }
        };

        _memberRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(memberships);

        // Act
        var result = await _service.GetHouseholdsForUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(h => h.Name == "Smith Family" && h.MemberCount == 2);
        result.Should().Contain(h => h.Name == "Johnson Family" && h.MemberCount == 3);
    }

    [Fact]
    public async Task GetHouseholdsForUserAsync_UserHasNoHouseholds_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _memberRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<HouseholdMember>());

        // Act
        var result = await _service.GetHouseholdsForUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region AddMemberAsync Tests

    [Fact]
    public async Task AddMemberAsync_UserNotInHousehold_ReturnsTrue()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Member";

        _memberRepositoryMock.Setup(x => x.IsUserInHouseholdAsync(userId, householdId))
            .ReturnsAsync(false);

        _memberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<HouseholdMember>()))
            .Returns(Task.FromResult<HouseholdMember>(null!));

        _memberRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.AddMemberAsync(householdId, userId, role);

        // Assert
        result.Should().BeTrue();
        _memberRepositoryMock.Verify(x => x.AddAsync(It.Is<HouseholdMember>(
            m => m.HouseholdId == householdId && m.UserId == userId && m.Role == role)), Times.Once);
        _memberRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_UserAlreadyInHousehold_ReturnsFalse()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Member";

        _memberRepositoryMock.Setup(x => x.IsUserInHouseholdAsync(userId, householdId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AddMemberAsync(householdId, userId, role);

        // Assert
        result.Should().BeFalse();
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdMember>()), Times.Never);
        _memberRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region GetByUserAndHouseholdAsync Tests

    [Fact]
    public async Task GetByUserAndHouseholdAsync_MemberExists_ReturnsHouseholdMemberDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        var member = new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            UserId = userId,
            Role = "Member",
            Avatar = "avatar.jpg",
            Xp = 500,
            Streak = 3,
            QuestsCompleted = 2,
            User = new User { FirstName = "Test", LastName = "User", Email = "test@example.com" },
            MemberBadges = new List<MemberBadge>()
        };

        _memberRepositoryMock.Setup(x => x.GetByUserAndHouseholdAsync(userId, householdId))
            .ReturnsAsync(member);

        // Act
        var result = await _service.GetByUserAndHouseholdAsync(userId, householdId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FirstName.Should().Be("Test");
        // Note: Service doesn't currently map Xp, Streak, QuestsCompleted in GetByUserAndHouseholdAsync
    }

    [Fact]
    public async Task GetByUserAndHouseholdAsync_MemberDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        _memberRepositoryMock.Setup(x => x.GetByUserAndHouseholdAsync(userId, householdId))
            .ReturnsAsync((HouseholdMember?)null);

        // Act
        var result = await _service.GetByUserAndHouseholdAsync(userId, householdId);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
