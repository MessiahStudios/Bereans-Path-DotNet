using BereansPath.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BereansPath.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<SavedChurch> SavedChurches => Set<SavedChurch>();
    public DbSet<BookmarkNoteMemoir> BookmarkNoteMemoirs => Set<BookmarkNoteMemoir>();
    public DbSet<ChurchSuggestion> ChurchSuggestions => Set<ChurchSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.Property(b => b.Reference).HasMaxLength(200).IsRequired();
            entity.Property(b => b.PassageText).HasMaxLength(8000);
            entity.Property(b => b.Note).HasMaxLength(2000);
            entity.HasMany(b => b.Memoirs)
                .WithOne(m => m.Bookmark)
                .HasForeignKey(m => m.BookmarkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookmarkNoteMemoir>(entity =>
        {
            entity.Property(m => m.NoteText).HasMaxLength(2000).IsRequired();
            entity.HasIndex(m => m.BookmarkId);
        });

        modelBuilder.Entity<SavedChurch>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(300).IsRequired();
            entity.Property(c => c.OsmId).HasMaxLength(100);
        });

        modelBuilder.Entity<ChurchSuggestion>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(300).IsRequired();
            entity.Property(c => c.City).HasMaxLength(200);
            entity.Property(c => c.Website).HasMaxLength(500);
            entity.Property(c => c.Denomination).HasMaxLength(120);
            entity.Property(c => c.Reason).HasMaxLength(2000).IsRequired();
            entity.Property(c => c.ContactEmail).HasMaxLength(200);
        });

        modelBuilder.Entity<Bookmark>().HasData(
            new Bookmark
            {
                Id = 1,
                Reference = "John 3:16",
                PassageText = "For God so loved the world, that he gave his only Son...",
                Note = "Seed bookmark — replace after first run",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
