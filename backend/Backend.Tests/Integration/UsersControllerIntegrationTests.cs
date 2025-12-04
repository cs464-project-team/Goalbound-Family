using System.Net;
using System.Net.Http.Json;
using GoalboundFamily.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Backend.Tests.Integration;

public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Use the Testing environment so Program.cs loads .env.testing and connects to test DB
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");
        });
    }

    [Fact(Skip = "Integration test requires Supabase configuration (.env.testing with SUPABASE_URL)")]
    public async Task CreateUser_Then_GetByEmail_Then_DeleteUser_WorksEndToEnd()
    {
        var client = _factory.CreateClient();

        var userId = Guid.NewGuid();
        var email = $"ci-test-{userId:N}@example.com";

        var createRequest = new CreateUserRequest
        {
            Id = userId,              // Supabase Auth UUID (for test we just use a new Guid)
            FirstName = "CI",
            LastName = "TestUser",
            Email = email
        };

        UserDto? createdUser = null;

        try
        {
            // 1) Create user
            var createResponse = await client.PostAsJsonAsync("/api/users", createRequest);
            createResponse.EnsureSuccessStatusCode(); // expect 201 Created

            createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(createdUser);
            Assert.Equal(userId, createdUser!.Id);
            Assert.Equal(email, createdUser.Email);
            Assert.Equal("CI", createdUser.FirstName);
            Assert.Equal("TestUser", createdUser.LastName);

            // 2) Get user by email
            var getResponse = await client.GetAsync($"/api/users/email/{email}");
            getResponse.EnsureSuccessStatusCode(); // expect 200 OK

            var fetchedUser = await getResponse.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(fetchedUser);
            Assert.Equal(createdUser.Id, fetchedUser!.Id);
            Assert.Equal(createdUser.Email, fetchedUser.Email);
            Assert.Equal(createdUser.FirstName, fetchedUser.FirstName);
            Assert.Equal(createdUser.LastName, fetchedUser.LastName);
        }
        finally
        {
            // 3) Cleanup: delete the user if it was created
            if (createdUser is not null)
            {
                var deleteResponse = await client.DeleteAsync($"/api/users/{createdUser.Id}");

                // We don't want the test to fail *only* because cleanup failed,
                // so we just ensure it's a "good" status if possible.
                if (deleteResponse.StatusCode is not HttpStatusCode.NoContent
                    and not HttpStatusCode.NotFound)
                {
                    // You could log something here if needed
                }
            }
        }
    }
}
