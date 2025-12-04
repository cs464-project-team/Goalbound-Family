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

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new UserService(_userRepositoryMock.Object);
    }

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserByEmailAsync Tests

    [Fact]
    public async Task GetUserByEmailAsync_UserExists_ReturnsUserDto()
    {
        // Arrange
        var email = "jane.smith@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetUserByEmailAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_UsersExist_ReturnsListOfUserDtos()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com" },
            new User { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Williams", Email = "bob@example.com" },
            new User { Id = Guid.NewGuid(), FirstName = "Charlie", LastName = "Brown", Email = "charlie@example.com" }
        };

        _userRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(u => u.FirstName).Should().Contain(new[] { "Alice", "Bob", "Charlie" });
    }

    [Fact]
    public async Task GetAllUsersAsync_NoUsers_ReturnsEmptyList()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_ValidRequest_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateUserRequest
        {
            Id = userId,
            FirstName = "New",
            LastName = "User",
            Email = "newuser@example.com"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(It.IsAny<User>());

        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("User");
        result.Email.Should().Be("newuser@example.com");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Id = Guid.NewGuid(),
            FirstName = "Duplicate",
            LastName = "Email",
            Email = "duplicate@example.com"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateUserAsync(request)
        );

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_ValidRequest_ReturnsUpdatedUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Old",
            LastName = "Name",
            Email = "old@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateUserAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.Email.Should().Be("updated@example.com");

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "User"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.UpdateUserAsync(userId, request);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };

        var request = new UpdateUserRequest
        {
            Email = "existing@example.com"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateUserAsync(userId, request)
        );

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete@example.com"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(x => x.DeleteAsync(user))
            .Returns(Task.CompletedTask);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.DeleteAsync(user), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region EmailExistsAsync Tests

    [Fact]
    public async Task EmailExistsAsync_EmailExists_ReturnsTrue()
    {
        // Arrange
        var email = "exists@example.com";
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(true);

        // Act
        var result = await _service.EmailExistsAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_EmailDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var email = "notexists@example.com";
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(false);

        // Act
        var result = await _service.EmailExistsAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
