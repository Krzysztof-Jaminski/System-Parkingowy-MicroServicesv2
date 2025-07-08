using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReservationService.Models;
using ReservationService.Repositories;
using Xunit;

namespace ReservationService.Tests
{
    public class ReservationRepositoryTests
    {
        private ReservationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ReservationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;
            var context = new ReservationDbContext(options);
            context.Reservations.AddRange(
                new Reservation { Id = 1, UserId = 1, ParkingSpot = "A1", StartTime = System.DateTime.Now, EndTime = System.DateTime.Now.AddHours(1) },
                new Reservation { Id = 2, UserId = 2, ParkingSpot = "B2", StartTime = System.DateTime.Now, EndTime = System.DateTime.Now.AddHours(2) }
            );
            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllReservations()
        {
            var context = GetDbContext();
            var repo = new ReservationRepository(context);
            var reservations = await repo.GetAllAsync();
            Assert.Equal(2, reservations.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectReservation()
        {
            var context = GetDbContext();
            var repo = new ReservationRepository(context);
            var reservation = await repo.GetByIdAsync(1);
            Assert.NotNull(reservation);
            Assert.Equal("A1", reservation.ParkingSpot);
        }

        [Fact]
        public async Task AddAsync_AddsReservation()
        {
            var context = GetDbContext();
            var repo = new ReservationRepository(context);
            var newReservation = new Reservation { UserId = 3, ParkingSpot = "C3", StartTime = System.DateTime.Now, EndTime = System.DateTime.Now.AddHours(3) };
            var added = await repo.AddAsync(newReservation);
            Assert.True(added.Id > 0);
            Assert.Equal("C3", added.ParkingSpot);
        }

        [Fact]
        public async Task DeleteAsync_RemovesReservation()
        {
            var context = GetDbContext();
            var repo = new ReservationRepository(context);
            var result = await repo.DeleteAsync(1);
            Assert.True(result);
            Assert.Null(await repo.GetByIdAsync(1));
        }
    }
} 