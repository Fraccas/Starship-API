using System.Text.Json.Serialization;

public class Starship
{
    public int Id { get; set; }

    [JsonPropertyName("name")] // maps json properties names to our C# properties
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [JsonPropertyName("cost_in_credits")]
    public string CostInCredits { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public string Length { get; set; } = string.Empty;

    [JsonPropertyName("max_atmosphering_speed")]
    public string MaxAtmospheringSpeed { get; set; } = string.Empty;

    [JsonPropertyName("crew")]
    public string Crew { get; set; } = string.Empty;

    [JsonPropertyName("passengers")]
    public string Passengers { get; set; } = string.Empty;

    [JsonPropertyName("cargo_capacity")]
    public string CargoCapacity { get; set; } = string.Empty;

    [JsonPropertyName("consumables")]
    public string Consumables { get; set; } = string.Empty;

    [JsonPropertyName("hyperdrive_rating")]
    public string HyperdriveRating { get; set; } = string.Empty;

    [JsonPropertyName("MGLT")]
    public string MGLT { get; set; } = string.Empty;

    [JsonPropertyName("starship_class")]
    public string StarshipClass { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public DateTime SwapiCreated { get; set; }

    [JsonPropertyName("edited")]
    public DateTime SwapiEdited { get; set; }

    [JsonPropertyName("url")]
    public string SwapiUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
