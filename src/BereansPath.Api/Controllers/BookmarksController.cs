using BereansPath.Api.Data;
using BereansPath.Api.Dtos;
using BereansPath.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BereansPath.Api.Controllers;

[ApiController]
[Route("api/bookmarks")]
public class BookmarksController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookmarkDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await db.Bookmarks
            .AsNoTracking()
            .Include(b => b.Memoirs)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookmarkDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var bookmark = await db.Bookmarks
            .AsNoTracking()
            .Include(b => b.Memoirs)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bookmark is null) return NotFound();
        return Ok(ToDto(bookmark));
    }

    [HttpPost]
    public async Task<ActionResult<BookmarkDto>> Create([FromBody] CreateBookmarkRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var bookmark = new Bookmark
        {
            Reference = request.Reference.Trim(),
            PassageText = request.PassageText,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.Bookmarks.Add(bookmark);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = bookmark.Id }, ToDto(bookmark));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<BookmarkDto>> Update(int id, [FromBody] UpdateBookmarkRequest request, CancellationToken cancellationToken)
    {
        var bookmark = await db.Bookmarks
            .Include(b => b.Memoirs)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bookmark is null) return NotFound();

        var incoming = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        var previous = string.IsNullOrWhiteSpace(bookmark.Note) ? null : bookmark.Note.Trim();

        // Archive the prior note when a meaningfully different perspective is saved.
        if (!string.IsNullOrEmpty(previous) &&
            !string.Equals(previous, incoming, StringComparison.Ordinal))
        {
            db.BookmarkNoteMemoirs.Add(new BookmarkNoteMemoir
            {
                BookmarkId = bookmark.Id,
                NoteText = previous,
                ArchivedAt = DateTime.UtcNow
            });
        }

        bookmark.Note = incoming;
        await db.SaveChangesAsync(cancellationToken);

        await db.Entry(bookmark).Collection(b => b.Memoirs).LoadAsync(cancellationToken);
        return Ok(ToDto(bookmark));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var bookmark = await db.Bookmarks.FindAsync([id], cancellationToken);
        if (bookmark is null) return NotFound();

        db.Bookmarks.Remove(bookmark);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static BookmarkDto ToDto(Bookmark bookmark) =>
        new(
            bookmark.Id,
            bookmark.Reference,
            bookmark.PassageText,
            bookmark.Note,
            bookmark.CreatedAt,
            bookmark.Memoirs
                .OrderByDescending(m => m.ArchivedAt)
                .Select(m => new MemoirDto(m.Id, m.NoteText, m.ArchivedAt))
                .ToList());
}
