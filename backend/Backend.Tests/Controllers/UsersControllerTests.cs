using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GoalboundFamily.Api.Controllers;
using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_userServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOk_WithListOfUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new() { Id = Guid.NewGuid(), Email = "test1@example.com" },
            new() { Id = Guid.NewGuid(), Email = "test2@example.com" }
        };

        _userServiceMock
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedUsers = okResult!.Value as IEnumerable<UserDto>;
        returnedUsers.Should().NotBeNull();
        returnedUsers!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserById_UserExists_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto { Id = userId, Email = "user@example.com" };

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedUser = okResult!.Value as UserDto;
        returnedUser.Should().NotBeNull();
        returnedUser!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserById_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var newUserId = Guid.NewGuid();

        var request = new CreateUserRequest
        {
            // fill in required properties
            Email = "newuser@example.com"
        };

        var createdUser = new UserDto
        {
            Id = newUserId,
            Email = request.Email
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(request))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        var createdAt = result.Result as CreatedAtActionResult;
        createdAt.Should().NotBeNull();
        createdAt!.ActionName.Should().Be(nameof(UsersController.GetUserById));
        var returnedUser = createdAt.Value as UserDto;
        returnedUser.Should().NotBeNull();
        returnedUser!.Id.Should().Be(newUserId);
    }
}
