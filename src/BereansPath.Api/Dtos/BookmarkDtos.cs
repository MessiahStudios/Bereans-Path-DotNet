using System.ComponentModel.DataAnnotations;

namespace BereansPath.Api.Dtos;

public record BookmarkDto(int Id, string Reference, string? PassageText, string? Note, DateTime CreatedAt);

public class CreateBookmarkRequest
{
    [Required, MaxLength(200)]
    public string Reference { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string? PassageText { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }
}

public class CreateSavedChurchRequest
{
    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [MaxLength(100)]
    public string? OsmId { get; set; }
}
