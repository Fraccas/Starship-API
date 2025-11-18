using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarShipApi.Data;

namespace StarShipApi.Controllers
{
    // Can be overriden with this: [Route("api/starships")]
    [Route("api/[controller]")] // route is controller name minus the word "Controller" ("api/Starship")
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class StarshipController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StarshipController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/starship
        // PUBLIC
        [HttpGet]
        [AllowAnonymous] // no auth needed
        public async Task<ActionResult<List<Starship>>> GetStarships()
        {
            List<Starship> ships = await _context.Starships.ToListAsync();
            return Ok(ships);
        }

        // GET: api/starship/5
        // PUBLIC
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Starship>> GetStarship(int id)
        {
            Starship? ship = await _context.Starships.FindAsync(id);

            if (ship == null)
                return NotFound();

            return Ok(ship);
        }

        // POST: api/starship
        // (Admin-only later)
        [HttpPost]
        public async Task<ActionResult<Starship>> CreateStarship([FromBody] Starship starship)
        {
            // defensive: ignore Id from client; creates its own ID
            starship.Id = 0;

            _context.Starships.Add(starship);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStarship), new { id = starship.Id }, starship);
        }

        // PUT: api/starship/5
        // (Admin-only later)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStarship(int id, [FromBody] Starship updated)
        {
            if (id != updated.Id)
                return BadRequest("ID in URL and body must match.");

            bool exists = await _context.Starships.AnyAsync(s => s.Id == id);
            if (!exists)
                return NotFound();

            _context.Entry(updated).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Starships.AnyAsync(s => s.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/starship/5
        // (Admin-only later)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStarship(int id)
        {
            Starship? ship = await _context.Starships.FindAsync(id);
            if (ship == null)
                return NotFound();

            _context.Starships.Remove(ship);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
