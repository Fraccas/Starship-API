using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarShipApi.Controllers;
using StarShipApi.Data;
using StarShipApi.Models;
using StarShipApi.Tests.Helpers;
using System.Security.Claims;

namespace StarShipApi.Tests
{
    public class FavoriteStarshipControllerTests
    {
        private async Task<(AppDbContext context, FavoriteStarshipController controller)> BuildControllerWithDataAsync()
        {
            var context = TestDbContextFactory.CreateInMemoryContext();

            // Seed base starships
            var xWing = new Starship { Name = "X-wing", Model = "T-65" };
            var falcon = new Starship { Name = "Millennium Falcon", Model = "YT-1300" };
            context.Starships.AddRange(xWing, falcon);
            await context.SaveChangesAsync();

            // Seed favorites for two different users
            var user1Id = "user-1";
            var user2Id = "user-2";

            context.FavoriteStarships.AddRange(
                new FavoriteStarship
                {
                    UserId = user1Id,
                    StarshipId = xWing.Id,
                    Nickname = "My X-wing"
                },
                new FavoriteStarship
                {
                    UserId = user2Id,
                    StarshipId = falcon.Id,
                    Nickname = "Not visible to user-1"
                }
            );

            await context.SaveChangesAsync();

            var controller = new FavoriteStarshipController(context);

            // Fake an authenticated user: user-1
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user1Id)
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return (context, controller);
        }

        [Fact]
        public async Task GetFavorites_ReturnsOnlyFavoritesForCurrentUser()
        {
            // Arrange
            var (_, controller) = await BuildControllerWithDataAsync();

            // Act
            var result = await controller.GetFavorites();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var favorites = Assert.IsAssignableFrom<List<FavoriteStarship>>(okResult.Value);

            Assert.Single(favorites);
            Assert.Equal("My X-wing", favorites[0].Nickname);
        }
    }
}
