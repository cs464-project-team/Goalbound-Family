using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

/// <summary>
/// Service for managing Supabase authentication via REST API
/// </summary>
public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _supabaseAnonKey;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SupabaseAuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _supabaseUrl = configuration["VITE_SUPABASE_URL"]
            ?? throw new InvalidOperationException("VITE_SUPABASE_URL not configured");
        _supabaseAnonKey = configuration["VITE_SUPABASE_ANON_KEY"]
            ?? throw new InvalidOperationException("VITE_SUPABASE_ANON_KEY not configured");
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseAnonKey);
    }

    public async Task<SupabaseAuthResponse> SignInWithPasswordAsync(string email, string password)
    {
        try
        {
            var requestBody = new
            {
                email,
                password,
                grant_type = "password"
            };

            var response = await PostAuthRequestAsync("/auth/v1/token?grant_type=password", requestBody);
            return await DeserializeAuthResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign in user with email: {Email}", email);
            throw;
        }
    }

    public async Task<SupabaseAuthResponse> SignUpAsync(string email, string password)
    {
        try
        {
            var requestBody = new
            {
                email,
                password
            };

            var response = await PostAuthRequestAsync("/auth/v1/signup", requestBody);
            return await DeserializeAuthResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign up user with email: {Email}", email);
            throw;
        }
    }

    public async Task<SupabaseAuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var requestBody = new
            {
                refresh_token = refreshToken,
                grant_type = "refresh_token"
            };

            var response = await PostAuthRequestAsync("/auth/v1/token?grant_type=refresh_token", requestBody);
            return await DeserializeAuthResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            throw;
        }
    }

    public async Task<SupabaseUser> GetUserFromTokenAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_supabaseUrl}/auth/v1/user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("apikey", _supabaseAnonKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get user from token. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to get user: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<SupabaseUser>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user ?? throw new InvalidOperationException("Failed to deserialize user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user from token");
            throw;
        }
    }

    public async Task SignOutAsync(string refreshToken)
    {
        try
        {
            var requestBody = new
            {
                refresh_token = refreshToken
            };

            await PostAuthRequestAsync("/auth/v1/logout", requestBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign out user");
            // Don't throw - we still want to clear the cookie even if Supabase signout fails
        }
    }

    private async Task<HttpResponseMessage> PostAuthRequestAsync(string endpoint, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_supabaseUrl}{endpoint}")
        {
            Content = content
        };
        request.Headers.Add("apikey", _supabaseAnonKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Supabase auth request failed. Status: {Status}, Error: {Error}",
                response.StatusCode, errorContent);

            // Try to extract error message from Supabase response
            try
            {
                var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorJson.TryGetProperty("error_description", out var errorDesc))
                {
                    throw new InvalidOperationException(errorDesc.GetString() ?? "Authentication failed");
                }
                if (errorJson.TryGetProperty("msg", out var msg))
                {
                    throw new InvalidOperationException(msg.GetString() ?? "Authentication failed");
                }
            }
            catch (JsonException)
            {
                // If we can't parse the error, use a generic message
            }

            throw new InvalidOperationException($"Authentication failed: {response.StatusCode}");
        }

        return response;
    }

    private async Task<SupabaseAuthResponse> DeserializeAuthResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        var authResponse = JsonSerializer.Deserialize<SupabaseAuthResponse>(content, options);

        if (authResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize auth response");
        }

        return authResponse;
    }
}
