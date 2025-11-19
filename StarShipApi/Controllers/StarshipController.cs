using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarShipApi.Data;

namespace StarShipApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StarshipController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StarshipController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        // GET: api/starship (PUBLIC)
        // ------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Starship>>> GetStarships()
        {
            return Ok(await _context.Starships.ToListAsync());
        }

        // ------------------------------------------------------------
        // GET: api/starship/5 (PUBLIC)
        // ------------------------------------------------------------
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Starship>> GetStarship(int id)
        {
            var ship = await _context.Starships.FindAsync(id);
            if (ship == null)
                return NotFound();

            return Ok(ship);
        }

        // ------------------------------------------------------------
        // POST: api/starship (ADMIN ONLY)
        // ------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Starship>> CreateStarship([FromBody] Starship starship)
        {
            starship.Id = 0;
            starship.CreatedAt = DateTime.UtcNow;

            _context.Starships.Add(starship);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStarship), new { id = starship.Id }, starship);
        }

        // ------------------------------------------------------------
        // PUT: api/starship/5 (ADMIN ONLY)
        // ------------------------------------------------------------
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStarship(int id, [FromBody] Starship updated)
        {
            var existing = await _context.Starships.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Update only allowed fields
            _context.Entry(existing).CurrentValues.SetValues(updated);

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // ------------------------------------------------------------
        // DELETE: api/starship/5 (ADMIN ONLY)
        // ------------------------------------------------------------
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStarship(int id)
        {
            var ship = await _context.Starships.FindAsync(id);
            if (ship == null)
                return NotFound();

            _context.Starships.Remove(ship);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }
    }
}
