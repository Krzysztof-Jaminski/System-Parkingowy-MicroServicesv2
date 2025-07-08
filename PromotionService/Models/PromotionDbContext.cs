using Microsoft.EntityFrameworkCore;

namespace PromotionService.Models
{
    public class PromotionDbContext : DbContext
    {
        public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options) { }
        public DbSet<Promotion> Promotions { get; set; }
    }
} 