namespace BereansPath.Api.Models;

public class Bookmark
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? PassageText { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
