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
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookmarkDto(b.Id, b.Reference, b.PassageText, b.Note, b.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookmarkDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var bookmark = await db.Bookmarks.FindAsync([id], cancellationToken);
        if (bookmark is null) return NotFound();

        return Ok(new BookmarkDto(bookmark.Id, bookmark.Reference, bookmark.PassageText, bookmark.Note, bookmark.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<BookmarkDto>> Create([FromBody] CreateBookmarkRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var bookmark = new Bookmark
        {
            Reference = request.Reference.Trim(),
            PassageText = request.PassageText,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        db.Bookmarks.Add(bookmark);
        await db.SaveChangesAsync(cancellationToken);

        var dto = new BookmarkDto(bookmark.Id, bookmark.Reference, bookmark.PassageText, bookmark.Note, bookmark.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = bookmark.Id }, dto);
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
}
