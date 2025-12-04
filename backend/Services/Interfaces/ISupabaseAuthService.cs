using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

/// <summary>
/// Service interface for Supabase authentication operations
/// </summary>
public interface ISupabaseAuthService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<SupabaseAuthResponse> SignInWithPasswordAsync(string email, string password);

    /// <summary>
    /// Register new user with email and password
    /// </summary>
    Task<SupabaseAuthResponse> SignUpAsync(string email, string password);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<SupabaseAuthResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Get user information from access token
    /// </summary>
    Task<SupabaseUser> GetUserFromTokenAsync(string accessToken);

    /// <summary>
    /// Sign out user (revoke refresh token)
    /// </summary>
    Task SignOutAsync(string refreshToken);
}
