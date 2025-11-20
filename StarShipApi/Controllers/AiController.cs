using Microsoft.AspNetCore.Mvc;
using StarShipApi.Data;
using System.Text.Json;

namespace StarShipApi.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public AiController(AppDbContext context, IHttpClientFactory httpFactory, IConfiguration config)
        {
            _context = context;
            _httpFactory = httpFactory;
            _config = config;
        }

        [HttpPost("starship-question")]
        public async Task<IActionResult> AskStarshipQuestion([FromBody] StarshipQuestionRequest req)
        {
            var ship = await _context.Starships.FindAsync(req.StarshipId);
            if (ship == null)
                return BadRequest(new { error = "Starship not found." });

            var systemPrompt =
                "You are a Star Wars starship expert. Answer user questions using real ship specs, " +
                "Star Wars lore, and technical perspective. Keep answers under 150 words.";

            var userPrompt =
                $"Starship: {ship.Name}\n" +
                $"Model: {ship.Model}\n" +
                $"Class: {ship.StarshipClass}\n" +
                $"Manufacturer: {ship.Manufacturer}\n" +
                $"Speed: {ship.MaxAtmospheringSpeed}\n" +
                $"Crew: {ship.Crew}\n\n" +
                $"User question: {req.Question}";

            var openAiKey = _config["OpenAI:Key"];
            if (string.IsNullOrWhiteSpace(openAiKey))
                return StatusCode(500, new { error = "OpenAI API key missing." });

            var http = _httpFactory.CreateClient();
            var body = new
            {
                model = "gpt-4o-mini",   // Cheapest, fast, and good quality
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {openAiKey}");
            request.Content = JsonContent.Create(body);

            var response = await http.SendAsync(request);

            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("OpenAI raw response:");
            Console.WriteLine(raw);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            var answer = json
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Ok(new { answer });
        }
    }

    public class StarshipQuestionRequest
    {
        public int StarshipId { get; set; }
        public string StarshipName { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
    }
}
