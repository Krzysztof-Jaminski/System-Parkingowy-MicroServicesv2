using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Models;
using Xunit;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ReservationService.Tests
{
    public class ReservationsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _userClient;
        private readonly HttpClient _promotionClient;
        private readonly string _userServiceUrl;
        private readonly string _promotionServiceUrl;

        public ReservationsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            _userServiceUrl = config["ExternalServices:UserService"] ?? "http://localhost:5001";
            _promotionServiceUrl = config["ExternalServices:PromotionService"] ?? "http://localhost:5003";
            _userClient = new HttpClient { BaseAddress = new Uri(_userServiceUrl) };
            _promotionClient = new HttpClient { BaseAddress = new Uri(_promotionServiceUrl) };
        }

        private async Task<int> CreateTestUserAsync()
        {
            var user = new { Name = "TestUser_" + Guid.NewGuid(), Email = $"test{Guid.NewGuid()}@mail.com" };
            var resp = await _userClient.PostAsJsonAsync("/api/users", user);
            resp.EnsureSuccessStatusCode();
            var created = await resp.Content.ReadFromJsonAsync<UserDTO>();
            return created.Id;
        }

        private async Task DeleteTestUserAsync(int userId)
        {
            await _userClient.DeleteAsync($"/api/users/{userId}");
        }

        private void ResetAndSeedDb()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
            db.Reservations.RemoveRange(db.Reservations);
            db.SaveChanges();
        }

        private async Task<string> CreateTestPromotionAsync()
        {
            var code = "PROMO_" + Guid.NewGuid();
            var promo = new { Name = code, Description = "Test Promo", DiscountPercent = 10, ValidFrom = DateTime.Now, ValidTo = DateTime.Now.AddDays(1) };
            var resp = await _promotionClient.PostAsJsonAsync("/api/promotions", promo);
            resp.EnsureSuccessStatusCode();
            return code;
        }

        private async Task DeleteTestPromotionAsync(string code)
        {
            var getResp = await _promotionClient.GetAsync($"/api/promotions/code/{code}");
            if (!getResp.IsSuccessStatusCode) return;
            var promo = await getResp.Content.ReadFromJsonAsync<PromotionDTO>();
            await _promotionClient.DeleteAsync($"/api/promotions/{promo.Id}");
        }

        private async Task<string> CreateTestPromotionWithMinHoursAsync(double minHours)
        {
            var code = "PROMO_" + Guid.NewGuid();
            var promo = new { Name = code, Description = "Test Promo", DiscountPercent = 10, ValidFrom = DateTime.Now, ValidTo = DateTime.Now.AddDays(1), MinHours = minHours };
            var resp = await _promotionClient.PostAsJsonAsync("/api/promotions", promo);
            resp.EnsureSuccessStatusCode();
            return code;
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/reservations");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostAndGetById_Works()
        {
            int userId = await CreateTestUserAsync();
            int reservationId = 0;
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "C3", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(3) };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                postResponse.EnsureSuccessStatusCode();
                var created = await postResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.NotNull(created);
                reservationId = created.Id;

                var getResponse = await _client.GetAsync($"/api/reservations/{created.Id}");
                getResponse.EnsureSuccessStatusCode();
                var fetched = await getResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.Equal("C3", fetched.ParkingSpot);
                Assert.Equal(userId, fetched.UserId);
            }
            finally
            {
                if (reservationId != 0)
                    await _client.DeleteAsync($"/api/reservations/{reservationId}");
                await DeleteTestUserAsync(userId);
            }
        }

        [Fact]
        public async Task CannotAddReservationForNonexistentUser()
        {
            var reservation = new ReservationDTO { UserId = -9999, ParkingSpot = "A1", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
            var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
            Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
        }

        [Fact]
        public async Task CannotAddReservationWithNonexistentPromotion()
        {
            int userId = await CreateTestUserAsync();
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "B2", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(2), PromotionCode = "NONEXISTENT" };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
            }
            finally
            {
                await DeleteTestUserAsync(userId);
            }
        }

        [Fact]
        public async Task CanAddReservationWithExistingPromotion()
        {
            int userId = await CreateTestUserAsync();
            string promoCode = await CreateTestPromotionAsync();
            int reservationId = 0;
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "D4", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(2), PromotionCode = promoCode };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                postResponse.EnsureSuccessStatusCode();
                var created = await postResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.NotNull(created);
                reservationId = created.Id;
                Assert.Equal(promoCode, created.PromotionCode);
            }
            finally
            {
                if (reservationId != 0)
                    await _client.DeleteAsync($"/api/reservations/{reservationId}");
                await DeleteTestUserAsync(userId);
                await DeleteTestPromotionAsync(promoCode);
            }
        }

        [Fact]
        public async Task CannotAddReservationIfDoesNotMeetPromotionMinHours()
        {
            int userId = await CreateTestUserAsync();
            string promoCode = await CreateTestPromotionWithMinHoursAsync(5);
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "E5", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(2), PromotionCode = promoCode };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
                var msg = await postResponse.Content.ReadAsStringAsync();
                Assert.Contains("minimum hours", msg);
            }
            finally
            {
                await DeleteTestUserAsync(userId);
                await DeleteTestPromotionAsync(promoCode);
            }
        }

        [Fact]
        public async Task CanAddReservationIfMeetsPromotionMinHours()
        {
            int userId = await CreateTestUserAsync();
            string promoCode = await CreateTestPromotionWithMinHoursAsync(2);
            int reservationId = 0;
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "F6", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(3), PromotionCode = promoCode };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                if (!postResponse.IsSuccessStatusCode)
                {
                    var error = await postResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Status: {postResponse.StatusCode}, Body: {error}");
                }
                var created = await postResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.NotNull(created);
                reservationId = created.Id;
            }
            finally
            {
                if (reservationId != 0)
                    await _client.DeleteAsync($"/api/reservations/{reservationId}");
                await DeleteTestUserAsync(userId);
                await DeleteTestPromotionAsync(promoCode);
            }
        }

        [Fact]
        public async Task ReservationCostWithoutPromotion_IsBaseRateTimesHours()
        {
            int userId = await CreateTestUserAsync();
            int reservationId = 0;
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "G7", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(5) };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                postResponse.EnsureSuccessStatusCode();
                var created = await postResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.NotNull(created);
                reservationId = created.Id;
                Assert.Equal(50m, created.Cost); // 10 * 5 * 1
            }
            finally
            {
                if (reservationId != 0)
                    await _client.DeleteAsync($"/api/reservations/{reservationId}");
                await DeleteTestUserAsync(userId);
            }
        }

        [Fact]
        public async Task ReservationCostWith20PercentPromotion_IsCorrect()
        {
            int userId = await CreateTestUserAsync();
            int reservationId = 0;
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "H8", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(12), PromotionCode = "PROMO12H" };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                postResponse.EnsureSuccessStatusCode();
                var created = await postResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.NotNull(created);
                reservationId = created.Id;
                Assert.Equal(96m, created.Cost); // 10 * 12 * 0.8
            }
            finally
            {
                if (reservationId != 0)
                    await _client.DeleteAsync($"/api/reservations/{reservationId}");
                await DeleteTestUserAsync(userId);
            }
        }

        [Fact]
        public async Task ReservationCostWith50PercentPromotion_IsCorrect()
        {
            int userId = await CreateTestUserAsync();
            int reservationId = 0;
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "I9", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(80), PromotionCode = "PROMO72H" };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                postResponse.EnsureSuccessStatusCode();
                var created = await postResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.NotNull(created);
                reservationId = created.Id;
                Assert.Equal(400m, created.Cost); // 10 * 80 * 0.5
            }
            finally
            {
                if (reservationId != 0)
                    await _client.DeleteAsync($"/api/reservations/{reservationId}");
                await DeleteTestUserAsync(userId);
            }
        }

        [Fact]
        public async Task CanAddReservationIfDurationEqualsPromotionMinHours()
        {
            int userId = await CreateTestUserAsync();
            string promoCode = "PROMO12H";
            int reservationId = 0;
            try
            {
                var reservation = new ReservationDTO { UserId = userId, ParkingSpot = "J10", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(12), PromotionCode = promoCode };
                var postResponse = await _client.PostAsJsonAsync("/api/reservations", reservation);
                postResponse.EnsureSuccessStatusCode();
                var created = await postResponse.Content.ReadFromJsonAsync<ReservationDTO>();
                Assert.NotNull(created);
                reservationId = created.Id;
                Assert.Equal(96m, created.Cost); // 10 * 12 * 0.8
            }
            finally
            {
                if (reservationId != 0)
                    await _client.DeleteAsync($"/api/reservations/{reservationId}");
                await DeleteTestUserAsync(userId);
            }
        }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
} 