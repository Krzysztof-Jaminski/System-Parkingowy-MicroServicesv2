using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UserService.Models;
using Xunit;

namespace UserService.Tests
{
    public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private void ResetAndSeedDb()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            db.Users.RemoveRange(db.Users);
            db.SaveChanges();
            db.Users.AddRange(
                new User { Name = "Test User 1", Email = "test1@email.com" },
                new User { Name = "Test User 2", Email = "test2@email.com" }
            );
            db.SaveChanges();
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            // Tworzymy unikalnego usera
            var uniqueEmail = $"integration_{Guid.NewGuid()}@email.com";
            var user = new UserDTO { Name = "Integration Test", Email = uniqueEmail };
            var postResponse = await _client.PostAsJsonAsync("/api/users", user);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(created);

            // Pobieramy wszystkich userów
            var response = await _client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var users = await response.Content.ReadFromJsonAsync<UserDTO[]>();
            Assert.NotNull(users);
            Assert.Contains(users, u => u.Id == created.Id && u.Email == uniqueEmail);

            // Sprzątamy po sobie
            var deleteResponse = await _client.DeleteAsync($"/api/users/{created.Id}");
            deleteResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PostAndGetById_Works()
        {
            // Tworzymy unikalnego usera
            var uniqueEmail = $"integration_{Guid.NewGuid()}@email.com";
            var user = new UserDTO { Name = "Integration Test", Email = uniqueEmail };
            var postResponse = await _client.PostAsJsonAsync("/api/users", user);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(created);

            // Pobieramy usera po ID
            var getResponse = await _client.GetAsync($"/api/users/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<UserDTO>();
            Assert.Equal("Integration Test", fetched.Name);
            Assert.Equal(uniqueEmail, fetched.Email);

            // Sprzątamy po sobie
            var deleteResponse = await _client.DeleteAsync($"/api/users/{created.Id}");
            deleteResponse.EnsureSuccessStatusCode();
        }
    }
} 