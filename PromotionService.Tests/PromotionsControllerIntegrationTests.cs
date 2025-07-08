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
        public async Task GetAll_ReturnsOkAndContainsCreated()
        {
            // Tworzymy unikalną promocję
            var promo = new PromotionDTO
            {
                Name = $"Promo_{Guid.NewGuid()}",
                Description = "Test Desc",
                DiscountPercent = 25,
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.AddDays(2)
            };
            var postResponse = await _client.PostAsJsonAsync("/api/promotions", promo);
            postResponse.EnsureSuccessStatusCode();
            var created = await postResponse.Content.ReadFromJsonAsync<PromotionDTO>();
            Assert.NotNull(created);

            // Pobieramy wszystkie promocje
            var response = await _client.GetAsync("/api/promotions");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var promotions = await response.Content.ReadFromJsonAsync<PromotionDTO[]>();
            Assert.NotNull(promotions);
            Assert.Contains(promotions, p => p.Id == created.Id && p.Name == promo.Name);

            // Sprzątamy po sobie
            var deleteResponse = await _client.DeleteAsync($"/api/promotions/{created.Id}");
            deleteResponse.EnsureSuccessStatusCode();
        }
    }
} 