namespace StarShipApi.Services
{
    public interface ISwapiService
    {
        Task<List<Starship>> GetStarshipsAsync();
    }
}
