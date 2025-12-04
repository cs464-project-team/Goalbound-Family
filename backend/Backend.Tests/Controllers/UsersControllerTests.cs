using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using GoalboundFamily.Api.Controllers;
using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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

    // ---- helpers ----
    private void SetUserOnController(Guid userId)
    {
        var claims = new List<Claim> { new("sub", userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    private void SetUserOnController(string subClaimValue)
    {
        var claims = new List<Claim> { new("sub", subClaimValue) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    // ================= EXISTING STYLE TESTS + NEW ONES =================

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
    public async Task GetAllUsers_WhenServiceThrows_Returns500()
    {
        // Arrange
        _userServiceMock
            .Setup(s => s.GetAllUsersAsync())
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var objectResult = result.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while retrieving users");
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
        notFoundResult.Value.Should().Be($"User with ID {userId} not found");
    }

    [Fact]
    public async Task GetUserById_WhenServiceThrows_Returns500()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(userId))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var objectResult = result.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while retrieving the user");
    }

    [Fact]
    public async Task GetUserByEmail_UserExists_ReturnsOk()
    {
        // Arrange
        var email = "user@example.com";
        var user = new UserDto { Id = Guid.NewGuid(), Email = email };

        _userServiceMock
            .Setup(s => s.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedUser = okResult!.Value as UserDto;
        returnedUser.Should().NotBeNull();
        returnedUser!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmail_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var email = "missing@example.com";

        _userServiceMock
            .Setup(s => s.GetUserByEmailAsync(email))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
        notFoundResult.Value.Should().Be($"User with email {email} not found");
    }

    [Fact]
    public async Task GetUserByEmail_WhenServiceThrows_Returns500()
    {
        // Arrange
        var email = "user@example.com";

        _userServiceMock
            .Setup(s => s.GetUserByEmailAsync(email))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var objectResult = result.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while retrieving the user");
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var newUserId = Guid.NewGuid();

        var request = new CreateUserRequest
        {
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

    [Fact]
    public async Task CreateUser_WhenInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "duplicate@example.com" };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(request))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest!.StatusCode.Should().Be(400);
        badRequest.Value.Should().Be("Email already exists");
    }

    [Fact]
    public async Task CreateUser_WhenServiceThrows_Returns500()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "user@example.com" };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(request))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        var objectResult = result.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while creating the user");
    }

    [Fact]
    public async Task UpdateUser_NoSubClaim_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest();

        // Act
        var result = await _controller.UpdateUser(userId, request);

        // Assert
        var unauthorized = result.Result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
        unauthorized!.StatusCode.Should().Be(401);
        unauthorized.Value.Should().Be("Invalid user token");
    }

    [Fact]
    public async Task UpdateUser_InvalidGuidClaim_ReturnsUnauthorized()
    {
        // Arrange
        SetUserOnController("not-a-guid");
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest();

        // Act
        var result = await _controller.UpdateUser(userId, request);

        // Assert
        var unauthorized = result.Result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
        unauthorized!.StatusCode.Should().Be(401);
        unauthorized.Value.Should().Be("Invalid user token");
    }

    [Fact]
    public async Task UpdateUser_DifferentUser_ReturnsForbid()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var routeUserId = Guid.NewGuid();
        SetUserOnController(authenticatedUserId);
        var request = new UpdateUserRequest();

        // Act
        var result = await _controller.UpdateUser(routeUserId, request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdateUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserOnController(userId);
        var request = new UpdateUserRequest();

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(userId, request))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.UpdateUser(userId, request);

        // Assert
        var notFound = result.Result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
        notFound!.StatusCode.Should().Be(404);
        notFound.Value.Should().Be($"User with ID {userId} not found");
    }

    [Fact]
    public async Task UpdateUser_ValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserOnController(userId);
        var request = new UpdateUserRequest();
        var updatedUser = new UserDto { Id = userId, Email = "updated@example.com" };

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(userId, request))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(userId, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedUser = okResult!.Value as UserDto;
        returnedUser.Should().NotBeNull();
        returnedUser!.Id.Should().Be(userId);
        returnedUser.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task UpdateUser_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserOnController(userId);
        var request = new UpdateUserRequest();

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(userId, request))
            .ThrowsAsync(new InvalidOperationException("Invalid update"));

        // Act
        var result = await _controller.UpdateUser(userId, request);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest!.StatusCode.Should().Be(400);
        badRequest.Value.Should().Be("Invalid update");
    }

    [Fact]
    public async Task UpdateUser_ServiceThrows_Returns500()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserOnController(userId);
        var request = new UpdateUserRequest();

        _userServiceMock
            .Setup(s => s.UpdateUserAsync(userId, request))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.UpdateUser(userId, request);

        // Assert
        var objectResult = result.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while updating the user");
    }

    [Fact]
    public async Task DeleteUser_NoSubClaim_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var userId = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var unauthorized = result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
        unauthorized!.StatusCode.Should().Be(401);
        unauthorized.Value.Should().Be("Invalid user token");
    }

    [Fact]
    public async Task DeleteUser_InvalidGuidClaim_ReturnsUnauthorized()
    {
        // Arrange
        SetUserOnController("not-a-guid");
        var userId = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var unauthorized = result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
        unauthorized!.StatusCode.Should().Be(401);
        unauthorized.Value.Should().Be("Invalid user token");
    }

    [Fact]
    public async Task DeleteUser_DifferentUser_ReturnsForbid()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var routeUserId = Guid.NewGuid();
        SetUserOnController(authenticatedUserId);

        // Act
        var result = await _controller.DeleteUser(routeUserId);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserOnController(userId);

        _userServiceMock
            .Setup(s => s.DeleteUserAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var notFound = result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
        notFound!.StatusCode.Should().Be(404);
        notFound.Value.Should().Be($"User with ID {userId} not found");
    }

    [Fact]
    public async Task DeleteUser_Success_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserOnController(userId);

        _userServiceMock
            .Setup(s => s.DeleteUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteUser_ServiceThrows_Returns500()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserOnController(userId);

        _userServiceMock
            .Setup(s => s.DeleteUserAsync(userId))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while deleting the user");
    }

    [Fact]
    public async Task CheckEmailExists_ReturnsOkWithEmailAndFlag()
    {
        // Arrange
        var email = "check@example.com";
        _userServiceMock
            .Setup(s => s.EmailExistsAsync(email))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckEmailExists(email);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        var value = okResult!.Value!;
        var type = value.GetType();

        var emailProp = type.GetProperty("email");
        var existsProp = type.GetProperty("exists");

        emailProp.Should().NotBeNull("the anonymous object should have an 'email' property");
        existsProp.Should().NotBeNull("the anonymous object should have an 'exists' property");

        var emailValue = emailProp!.GetValue(value) as string;
        var existsValue = (bool?)existsProp!.GetValue(value);

        emailValue.Should().Be(email);
        existsValue.Should().BeTrue();
    }


    [Fact]
    public async Task CheckEmailExists_ServiceThrows_Returns500()
    {
        // Arrange
        var email = "check@example.com";
        _userServiceMock
            .Setup(s => s.EmailExistsAsync(email))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.CheckEmailExists(email);

        // Assert
        var objectResult = result.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while checking the email");
    }
}
