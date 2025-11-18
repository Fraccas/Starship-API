using Microsoft.AspNetCore.Mvc;
using StarShipApi.Controllers;
using StarShipApi.Data;
using StarShipApi.Tests.Helpers;

namespace StarShipApi.Tests
{
    public class StarshipControllerTests
    {
        private async Task<AppDbContext> SeedStarshipsAsync()
        {
            var context = TestDbContextFactory.CreateInMemoryContext();

            context.Starships.AddRange(
                new Starship { Name = "X-wing", Model = "T-65" },
                new Starship { Name = "Millennium Falcon", Model = "YT-1300" }
            );

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task GetStarships_ReturnsAllShips()
        {
            // Arrange
            var context = await SeedStarshipsAsync();
            var controller = new StarshipController(context);

            // Act
            var result = await controller.GetStarships();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var ships = Assert.IsAssignableFrom<List<Starship>>(okResult.Value);
            Assert.Equal(2, ships.Count);
        }

        [Fact]
        public async Task CreateStarship_InsertsShipIntoDatabase()
        {
            // Arrange
            var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new StarshipController(context);

            var newShip = new Starship
            {
                Name = "Test Ship",
                Model = "TS-01",
                Manufacturer = "Test Inc."
            };

            // Act
            var result = await controller.CreateStarship(newShip);

            // Assert: response
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdShip = Assert.IsType<Starship>(createdAt.Value);
            Assert.NotEqual(0, createdShip.Id);

            // Assert: actually in DB
            var fromDb = await context.Starships.FindAsync(createdShip.Id);
            Assert.NotNull(fromDb);
            Assert.Equal("Test Ship", fromDb!.Name);
        }
    }
}
