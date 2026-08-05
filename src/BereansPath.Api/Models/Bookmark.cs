namespace BereansPath.Api.Models;

public class Bookmark
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? PassageText { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<BookmarkNoteMemoir> Memoirs { get; set; } = [];
}

/// <summary>
/// Archived note when a bookmark note is updated — a prior perspective kept for reflection.
/// </summary>
public class BookmarkNoteMemoir
{
    public int Id { get; set; }
    public int BookmarkId { get; set; }
    public Bookmark Bookmark { get; set; } = null!;
    public string NoteText { get; set; } = string.Empty;
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
}
