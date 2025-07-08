using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Repositories;
using Xunit;
using System;

namespace UserService.Tests
{
    public class UserRepositoryTests
    {
        private UserDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            var context = new UserDbContext(options);
            context.Users.AddRange(
                new User { Id = 1, Name = "Test User 1", Email = "test1@email.com" },
                new User { Id = 2, Name = "Test User 2", Email = "test2@email.com" }
            );
            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var users = await repo.GetAllAsync();
            Assert.Equal(2, users.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectUser()
        {
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = await repo.GetByIdAsync(1);
            Assert.NotNull(user);
            Assert.Equal("Test User 1", user.Name);
        }

        [Fact]
        public async Task AddAsync_AddsUser()
        {
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var newUser = new User { Name = "New User", Email = "new@email.com" };
            var added = await repo.AddAsync(newUser);
            Assert.True(added.Id > 0);
            Assert.Equal("New User", added.Name);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUser()
        {
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var result = await repo.DeleteAsync(1);
            Assert.True(result);
            Assert.Null(await repo.GetByIdAsync(1));
        }
    }
}