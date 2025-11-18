using System.ComponentModel.DataAnnotations;

namespace StarShipApi.Models
{
    public class FavoriteStarship
    {
        public int Id { get; set; }

        // This matches ASP.NET Identity's user Id
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int StarshipId { get; set; }

        // Optional user-specific fields
        [MaxLength(100)]
        public string? Nickname { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Starship Starship { get; set; } = null!;
    }
}
