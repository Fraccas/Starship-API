using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StarShipApi.Models;

namespace StarShipApi.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser> // instead of base DbContext so we can use Auth
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Starship> Starships { get; set; }
        public DbSet<FavoriteStarship> FavoriteStarships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FavoriteStarship>()
                .HasOne(f => f.Starship)
                .WithMany()              // a Starship can belong to many favorites
                .HasForeignKey(f => f.StarshipId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
