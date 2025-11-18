namespace StarShipApi.Services
{
    public class SwapiService : ISwapiService
    {
        private readonly HttpClient _http;

        public SwapiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Starship>> GetStarshipsAsync()
        {
            string url = "https://swapi.info/api/starships";

            try
            {
                List<Starship>? result = await _http.GetFromJsonAsync<List<Starship>>(url);

                return result ?? new List<Starship>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SWAPI fetch failed: {ex.Message}");
                return new List<Starship>();
            }
        }
    }
}
