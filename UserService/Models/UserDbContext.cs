using Microsoft.EntityFrameworkCore;

namespace UserService.Models
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Jan Kowalski", Email = "jan.kowalski@email.com" },
                new User { Id = 2, Name = "Anna Nowak", Email = "anna.nowak@email.com" }
            );
        }
    }
} 