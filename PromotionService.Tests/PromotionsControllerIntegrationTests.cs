using System;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PromotionService.Models;
using Xunit;
using System.Threading.Tasks;

namespace PromotionService.Tests
{
    public class PromotionsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public PromotionsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostAndGetById_Works()
        {
            // Tworzymy unikalną promocję
            var promo = new PromotionDTO
            {
                Name = $"Promo_{Guid.NewGuid()}",
                Description = "Test Desc",
                DiscountPercent = 15,
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.AddDays(1)
            };
            var postResponse = await _client.PostAsJsonAsync("/api/promotions", promo);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<PromotionDTO>();
            Assert.NotNull(created);
            Assert.Equal(promo.Name, created.Name);

            // Pobieramy po ID
            var getResponse = await _client.GetAsync($"/api/promotions/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<PromotionDTO>();
            Assert.Equal(promo.Name, fetched.Name);

            // Sprzątamy po sobie
            var deleteResponse = await _client.DeleteAsync($"/api/promotions/{created.Id}");
            deleteResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/promotions");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task CreatePromotion_Valid_ReturnsCreated()
        {
            var promo = new PromotionDTO
            {
                Name = $"Promo_{Guid.NewGuid()}",
                Description = "Test Desc",
                DiscountPercent = 15,
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.AddDays(1)
            };
            var postResponse = await _client.PostAsJsonAsync("/api/promotions", promo);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<PromotionDTO>();
            Assert.NotNull(created);
            Assert.Equal(promo.Name, created.Name);
        }
        [Fact]
        public async Task UpdatePromotion_Valid_ReturnsOk()
        {
            var promo = new PromotionDTO
            {
                Name = $"Promo_{Guid.NewGuid()}",
                Description = "Test Desc",
                DiscountPercent = 15,
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.AddDays(1)
            };
            var postResponse = await _client.PostAsJsonAsync("/api/promotions", promo);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<PromotionDTO>();
            created.Description = "Updated Desc";
            var putResponse = await _client.PutAsJsonAsync($"/api/promotions/{created.Id}", created);
            putResponse.EnsureSuccessStatusCode();
            var updated = await putResponse.Content.ReadFromJsonAsync<PromotionDTO>();
            Assert.Equal("Updated Desc", updated.Description);
            await _client.DeleteAsync($"/api/promotions/{created.Id}");
        }
        [Fact]
        public async Task DeletePromotion_Valid_ReturnsNoContent()
        {
            var promo = new PromotionDTO
            {
                Name = $"Promo_{Guid.NewGuid()}",
                Description = "Test Desc",
                DiscountPercent = 15,
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.AddDays(1)
            };
            var postResponse = await _client.PostAsJsonAsync("/api/promotions", promo);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<PromotionDTO>();
            var deleteResponse = await _client.DeleteAsync($"/api/promotions/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
    }
} 