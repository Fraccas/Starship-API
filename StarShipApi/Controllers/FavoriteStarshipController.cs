using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarShipApi.Data;
using StarShipApi.Models;
using System.Security.Claims;

namespace StarShipApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FavoriteStarshipController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoriteStarshipController(AppDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            return userId;
        }

        // GET: api/favoritestarship
        [HttpGet]
        public async Task<ActionResult<List<FavoriteStarship>>> GetFavorites()
        {
            string userId = GetCurrentUserId();

            List<FavoriteStarship> favorites = await _context.FavoriteStarships
                .Include(f => f.Starship)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return Ok(favorites);
        }

        // POST: api/favoritestarship
        [HttpPost]
        public async Task<ActionResult<FavoriteStarship>> AddFavorite([FromBody] FavoriteStarship favorite)
        {
            string userId = GetCurrentUserId();

            bool starshipExists = await _context.Starships.AnyAsync(s => s.Id == favorite.StarshipId);
            if (!starshipExists)
                return BadRequest("StarshipId is invalid.");

            favorite.Id = 0; // DB set ID upon insert
            favorite.UserId = userId;
            favorite.CreatedAt = DateTime.UtcNow;

            _context.FavoriteStarships.Add(favorite);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFavorites), new { id = favorite.Id }, favorite);
        }

        // PUT: api/favoritestarship/5  (update nickname/notes)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFavorite(int id, [FromBody] FavoriteStarship updated)
        {
            string userId = GetCurrentUserId();

            FavoriteStarship? existing = await _context.FavoriteStarships
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (existing == null)
                return NotFound();

            // Only allow editing user-specific fields
            existing.Nickname = updated.Nickname;
            existing.Notes = updated.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/favoritestarship/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            string userId = GetCurrentUserId();

            FavoriteStarship? favorite = await _context.FavoriteStarships
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (favorite == null)
                return NotFound();

            _context.FavoriteStarships.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
