namespace BereansPath.Api.Models;

public class SavedChurch
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? OsmId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
