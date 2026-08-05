namespace BereansPath.Api.Models;

public class ChurchSuggestion
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Website { get; set; }
    public string? Denomination { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
