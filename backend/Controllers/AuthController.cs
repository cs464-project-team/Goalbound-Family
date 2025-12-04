using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

/// <summary>
/// Controller for authentication operations
/// Manages HttpOnly cookies for secure token storage
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    private const string RefreshTokenCookie = "sb_refresh_token";

    public AuthController(
        ISupabaseAuthService authService,
        IUserService userService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// Sets HttpOnly refresh token cookie
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Authenticate with Supabase
            var authResponse = await _authService.SignInWithPasswordAsync(request.Email, request.Password);

            // Validate auth response
            if (authResponse.User == null)
            {
                throw new InvalidOperationException("Failed to get user information from authentication service");
            }

            // Set HttpOnly cookie with refresh token
            SetRefreshTokenCookie(authResponse.RefreshToken);

            // Get user profile from database
            var user = await _userService.GetUserByIdAsync(Guid.Parse(authResponse.User.Id));

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = authResponse.AccessToken
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Login failed for email: {Email}", request.Email);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Sign up new user with email and password
    /// Sets HttpOnly refresh token cookie
    /// </summary>
    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> Signup([FromBody] SignupRequest request)
    {
        try
        {
            // Check if user already exists
            if (await _userService.EmailExistsAsync(request.Email))
            {
                return BadRequest(new { message = "A user with this email already exists" });
            }

            // Create auth user in Supabase
            var authResponse = await _authService.SignUpAsync(request.Email, request.Password);

            // Validate auth response
            if (authResponse.User == null)
            {
                throw new InvalidOperationException("Failed to get user information from authentication service");
            }

            // Set HttpOnly cookie with refresh token
            SetRefreshTokenCookie(authResponse.RefreshToken);

            // Create user profile in database
            var createUserRequest = new CreateUserRequest
            {
                Id = Guid.Parse(authResponse.User.Id),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            UserDto user;
            try
            {
                user = await _userService.CreateUserAsync(createUserRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user profile for {Email}, but Supabase user was created", request.Email);
                // Note: In production, you might want to implement a cleanup mechanism
                // to delete the Supabase auth user if profile creation fails
                throw;
            }

            _logger.LogInformation("User {Email} signed up successfully", request.Email);

            return CreatedAtAction(nameof(GetMe), null, new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = authResponse.AccessToken
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Signup failed for email: {Email}", request.Email);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during signup for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during signup" });
        }
    }

    /// <summary>
    /// Get current user information
    /// Refreshes access token using HttpOnly cookie
    /// Rotates refresh token for security
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponse>> GetMe()
    {
        try
        {
            // Get refresh token from cookie
            if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken))
            {
                // Return 204 No Content instead of 401 to avoid showing error in browser DevTools
                return NoContent();
            }

            // Refresh the session
            var authResponse = await _authService.RefreshTokenAsync(refreshToken);

            // Validate auth response
            if (authResponse.User == null)
            {
                throw new InvalidOperationException("Failed to get user information from authentication service");
            }

            // Rotate refresh token (set new cookie)
            SetRefreshTokenCookie(authResponse.RefreshToken);

            // Get user profile from database
            var user = await _userService.GetUserByIdAsync(Guid.Parse(authResponse.User.Id));

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = authResponse.AccessToken
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to refresh session");
            ClearRefreshTokenCookie();
            // Return 204 No Content instead of 401 to avoid showing error in browser DevTools
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during session refresh");
            ClearRefreshTokenCookie();
            return StatusCode(500, new { message = "An error occurred while validating session" });
        }
    }

    /// <summary>
    /// Refresh access token using HttpOnly cookie
    /// Returns new access token without rotating refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        try
        {
            // Get refresh token from cookie
            if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "No refresh token found" });
            }

            // Refresh the session
            var authResponse = await _authService.RefreshTokenAsync(refreshToken);

            // Validate auth response
            if (authResponse.User == null)
            {
                throw new InvalidOperationException("Failed to get user information from authentication service");
            }

            // Get user profile from database
            var user = await _userService.GetUserByIdAsync(Guid.Parse(authResponse.User.Id));

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = authResponse.AccessToken
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to refresh access token");
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(500, new { message = "An error occurred while refreshing token" });
        }
    }

    /// <summary>
    /// Logout current user
    /// Clears HttpOnly cookie and revokes refresh token
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        try
        {
            // Get refresh token from cookie
            if (Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) &&
                !string.IsNullOrEmpty(refreshToken))
            {
                // Revoke the refresh token in Supabase
                await _authService.SignOutAsync(refreshToken);
            }

            // Clear the cookie
            ClearRefreshTokenCookie();

            _logger.LogInformation("User logged out successfully");

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            // Still clear the cookie even if Supabase signout fails
            ClearRefreshTokenCookie();
            return Ok(new { message = "Logged out successfully" });
        }
    }

    /// <summary>
    /// Set HttpOnly refresh token cookie
    /// </summary>
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Only send over HTTPS
            SameSite = SameSiteMode.Lax, // Protect against CSRF
            Expires = DateTimeOffset.UtcNow.AddDays(7), // 7 day expiration
            Path = "/",
            IsEssential = true
        };

        Response.Cookies.Append(RefreshTokenCookie, refreshToken, cookieOptions);
    }

    /// <summary>
    /// Clear refresh token cookie
    /// </summary>
    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }
}
