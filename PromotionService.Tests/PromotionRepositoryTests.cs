using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PromotionService.Models;
using PromotionService.Repositories;
using Xunit;

namespace PromotionService.Tests
{
    public class PromotionRepositoryTests
    {
        private PromotionDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<PromotionDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            var context = new PromotionDbContext(options);
            context.Promotions.AddRange(
                new Promotion { Name = "Promo1", Description = "Desc1", DiscountPercent = 10, ValidFrom = DateTime.Now, ValidTo = DateTime.Now.AddDays(1) },
                new Promotion { Name = "Promo2", Description = "Desc2", DiscountPercent = 20, ValidFrom = DateTime.Now, ValidTo = DateTime.Now.AddDays(2) }
            );
            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllPromotions()
        {
            var context = GetDbContext();
            var repo = new PromotionRepository(context);
            var promotions = await repo.GetAllAsync();
            Assert.Equal(2, promotions.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectPromotion()
        {
            var context = GetDbContext();
            var repo = new PromotionRepository(context);
            var first = context.Promotions.First();
            var promotion = await repo.GetByIdAsync(first.Id);
            Assert.NotNull(promotion);
            Assert.Equal(first.Name, promotion.Name);
        }

        [Fact]
        public async Task AddAsync_AddsPromotion()
        {
            var context = GetDbContext();
            var repo = new PromotionRepository(context);
            var newPromotion = new Promotion { Name = "Promo3", Description = "Desc3", DiscountPercent = 30, ValidFrom = DateTime.Now, ValidTo = DateTime.Now.AddDays(3) };
            var added = await repo.AddAsync(newPromotion);
            Assert.True(added.Id > 0);
            Assert.Equal("Promo3", added.Name);
        }

        [Fact]
        public async Task DeleteAsync_RemovesPromotion()
        {
            var context = GetDbContext();
            var repo = new PromotionRepository(context);
            var first = context.Promotions.First();
            var result = await repo.DeleteAsync(first.Id);
            Assert.True(result);
            Assert.Null(await repo.GetByIdAsync(first.Id));
        }
    }
} 