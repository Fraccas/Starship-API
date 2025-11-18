using Microsoft.EntityFrameworkCore;
using StarShipApi.Services;

namespace StarShipApi.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, ISwapiService swapi)
        {
            // If there are already starships, do nothing
            if (await context.Starships.AnyAsync())
                return;

            List<Starship> starships = await swapi.GetStarshipsAsync();

            if (starships.Count == 0)
                return;

            await context.Starships.AddRangeAsync(starships);
            await context.SaveChangesAsync();
        }
    }
}
