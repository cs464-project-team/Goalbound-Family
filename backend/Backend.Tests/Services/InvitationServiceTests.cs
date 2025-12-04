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

public class InvitationServiceTests
{
    private readonly Mock<IInvitationRepository> _invitationRepositoryMock;
    private readonly Mock<IHouseholdMemberRepository> _memberRepositoryMock;
    private readonly Mock<IHouseholdRepository> _householdRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly InvitationService _service;

    public InvitationServiceTests()
    {
        _invitationRepositoryMock = new Mock<IInvitationRepository>();
        _memberRepositoryMock = new Mock<IHouseholdMemberRepository>();
        _householdRepositoryMock = new Mock<IHouseholdRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new InvitationService(
            _invitationRepositoryMock.Object,
            _memberRepositoryMock.Object,
            _householdRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsInvitationDto()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var invitedByUserId = Guid.NewGuid();

        var request = new CreateInvitationRequest
        {
            HouseholdId = householdId
        };

        _householdRepositoryMock.Setup(x => x.GetByIdAsync(householdId))
            .ReturnsAsync(new Household { Id = householdId, Name = "Test Household" });

        _userRepositoryMock.Setup(x => x.GetByIdAsync(invitedByUserId))
            .ReturnsAsync(new User { Id = invitedByUserId, Email = "test@example.com" });

        _invitationRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Invitation>()))
            .ReturnsAsync(new Invitation
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                InvitedByUserId = invitedByUserId,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsAccepted = false
            });

        _invitationRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.CreateAsync(request, invitedByUserId);

        // Assert
        result.Should().NotBeNull();
        result.HouseholdId.Should().Be(householdId);
        result.InvitedByUserId.Should().Be(invitedByUserId);
        result.Token.Should().NotBeNullOrEmpty();
        result.IsAccepted.Should().BeFalse();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        _invitationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Invitation>()), Times.Once);
        _invitationRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_HouseholdDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var invitedByUserId = Guid.NewGuid();

        var request = new CreateInvitationRequest
        {
            HouseholdId = householdId
        };

        _householdRepositoryMock.Setup(x => x.GetByIdAsync(householdId))
            .ReturnsAsync((Household?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request, invitedByUserId));
        exception.Message.Should().Contain(householdId.ToString());
    }

    [Fact]
    public async Task CreateAsync_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var householdId = Guid.NewGuid();
        var invitedByUserId = Guid.NewGuid();

        var request = new CreateInvitationRequest
        {
            HouseholdId = householdId
        };

        _householdRepositoryMock.Setup(x => x.GetByIdAsync(householdId))
            .ReturnsAsync(new Household { Id = householdId, Name = "Test Household" });

        _userRepositoryMock.Setup(x => x.GetByIdAsync(invitedByUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request, invitedByUserId));
        exception.Message.Should().Contain(invitedByUserId.ToString());
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_InvitationExists_ReturnsInvitationDto()
    {
        // Arrange
        var invitationId = Guid.NewGuid();
        var invitation = new Invitation
        {
            Id = invitationId,
            HouseholdId = Guid.NewGuid(),
            InvitedByUserId = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = false
        };

        _invitationRepositoryMock.Setup(x => x.GetByIdAsync(invitationId))
            .ReturnsAsync(invitation);

        // Act
        var result = await _service.GetAsync(invitationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(invitationId);
        result.HouseholdId.Should().Be(invitation.HouseholdId);
        result.IsAccepted.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_InvitationDoesNotExist_ReturnsNull()
    {
        // Arrange
        var invitationId = Guid.NewGuid();
        _invitationRepositoryMock.Setup(x => x.GetByIdAsync(invitationId))
            .ReturnsAsync((Invitation?)null);

        // Act
        var result = await _service.GetAsync(invitationId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AcceptAsync Tests

    [Fact]
    public async Task AcceptAsync_ValidInvitation_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var token = Guid.NewGuid().ToString();

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            InvitedByUserId = Guid.NewGuid(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = false
        };

        var request = new AcceptInvitationRequest
        {
            UserId = userId,
            Token = token
        };

        _invitationRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(invitation);

        _memberRepositoryMock.Setup(x => x.GetByUserAndHouseholdAsync(userId, householdId))
            .ReturnsAsync((HouseholdMember?)null);

        _memberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<HouseholdMember>()))
            .ReturnsAsync(new HouseholdMember
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                UserId = userId,
                Role = "Member"
            });

        _invitationRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Invitation>()))
            .Returns(Task.FromResult(1));

        _memberRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        _invitationRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.AcceptAsync(request);

        // Assert
        result.Should().BeTrue();
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdMember>()), Times.Once);
        _invitationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Invitation>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_InvitationNotFound_ReturnsFalse()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var request = new AcceptInvitationRequest
        {
            UserId = Guid.NewGuid(),
            Token = token
        };

        _invitationRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync((Invitation?)null);

        // Act
        var result = await _service.AcceptAsync(request);

        // Assert
        result.Should().BeFalse();
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdMember>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_ExpiredInvitation_ReturnsFalse()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            HouseholdId = Guid.NewGuid(),
            InvitedByUserId = Guid.NewGuid(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            IsAccepted = false
        };

        var request = new AcceptInvitationRequest
        {
            UserId = Guid.NewGuid(),
            Token = token
        };

        _invitationRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(invitation);

        // Act
        var result = await _service.AcceptAsync(request);

        // Assert
        result.Should().BeFalse();
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdMember>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_AlreadyAccepted_ReturnsFalse()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            HouseholdId = Guid.NewGuid(),
            InvitedByUserId = Guid.NewGuid(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = true // Already accepted
        };

        var request = new AcceptInvitationRequest
        {
            UserId = Guid.NewGuid(),
            Token = token
        };

        _invitationRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(invitation);

        // Act
        var result = await _service.AcceptAsync(request);

        // Assert
        result.Should().BeFalse();
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdMember>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_UserAlreadyMember_MarksInvitationAsAcceptedAndReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var token = Guid.NewGuid().ToString();

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            InvitedByUserId = Guid.NewGuid(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = false
        };

        var existingMember = new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            UserId = userId,
            Role = "Member"
        };

        var request = new AcceptInvitationRequest
        {
            UserId = userId,
            Token = token
        };

        _invitationRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(invitation);

        _memberRepositoryMock.Setup(x => x.GetByUserAndHouseholdAsync(userId, householdId))
            .ReturnsAsync(existingMember);

        _invitationRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Invitation>()))
            .Returns(Task.FromResult(1));

        _invitationRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.AcceptAsync(request);

        // Assert
        result.Should().BeTrue();
        _memberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HouseholdMember>()), Times.Never);
        _invitationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Invitation>()), Times.Once);
    }

    #endregion
}
