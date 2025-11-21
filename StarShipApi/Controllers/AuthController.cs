using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StarShipApi.Models.Dto;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StarShipApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedTest()
        {
            if (User == null) return BadRequest();
            if (User.Identity == null) return BadRequest();

            return Ok(new { message = "You are authenticated!", user = User.Identity.Name });
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // Check if email is empty
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { message = "Email is required." });

            // Validate email format
            var emailAttribute = new EmailAddressAttribute();
            if (!emailAttribute.IsValid(model.Email))
                return BadRequest(new { message = "Invalid email format." });

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email is already registered." });

            // Create user object
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            // Create user in Identity
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "User registered successfully!" });
        }


        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized();

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized();

            var token = await GenerateJwtAsync(user);
            return Ok(new { token });
        }

        private async Task<string> GenerateJwtAsync(IdentityUser user)
        {
            var jwtSection = _config.GetSection("Jwt");

            var secretKey = jwtSection.GetValue<string>("Key")
                ?? throw new InvalidOperationException("JWT signing key is missing.");

            if (secretKey.Length < 32)
                throw new InvalidOperationException("JWT signing key must be at least 32 characters long.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var email = user.Email ?? throw new InvalidOperationException("User email missing.");

            // FIXED — Use await, no blocking
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, email)
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var expiresMinutes = jwtSection.GetValue<double>("ExpireMinutes");

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // GET api/auth/seed-admin
        [HttpGet("seed-admin")]
        public async Task<IActionResult> SeedAdmin()
        {
            // Create Roles if missing
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            // Create default Admin
            var adminEmail = "admin@test.com";
            var admin = await _userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                await _userManager.CreateAsync(admin, "Password123!");
                await _userManager.AddToRoleAsync(admin, "Admin");
            }

            return Ok("Admin seeded!");
        }
    }
}
