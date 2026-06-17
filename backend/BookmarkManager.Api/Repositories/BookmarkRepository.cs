using BookmarkManager.Api.Data;
using BookmarkManager.Api.DTOs;
using BookmarkManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarkManager.Api.Repositories;

public class BookmarkRepository(BookmarkDbContext db) : IBookmarkRepository
{
    public async Task<Bookmark> CreateAsync(Bookmark bookmark)
    {
        bookmark.Id = Guid.NewGuid();
        db.Bookmarks.Add(bookmark);
        await db.SaveChangesAsync();
        return bookmark;
    }

    public async Task<IEnumerable<Bookmark>> GetAllAsync() =>
        await db.Bookmarks.OrderBy(b => b.CreatedAt).ToListAsync();

    public async Task<Bookmark?> GetByIdAsync(Guid id) =>
        await db.Bookmarks.FindAsync(id);

    public async Task<Bookmark?> UpdateAsync(Bookmark bookmark)
    {
        db.Bookmarks.Update(bookmark);
        await db.SaveChangesAsync();
        return bookmark;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bookmark = await db.Bookmarks.FindAsync(id);
        if (bookmark is null) return false;
        db.Bookmarks.Remove(bookmark);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNormalizedUrlAsync(string normalizedUrl) =>
        await db.Bookmarks.AnyAsync(b =>
            b.Url.ToLower().Trim() == normalizedUrl);

    public async Task<Bookmark?> GetByNormalizedUrlAsync(string normalizedUrl) =>
        await db.Bookmarks.FirstOrDefaultAsync(b =>
            b.Url.ToLower().Trim() == normalizedUrl);

    public async Task<IEnumerable<Bookmark>> GetFilteredAsync(string? tag, bool? isRead, string? keyword)
    {
        var bookmarks = await db.Bookmarks.OrderBy(b => b.CreatedAt).ToListAsync();
        IEnumerable<Bookmark> result = bookmarks;

        if (tag is not null)
            result = result.Where(b => b.Tags.Any(t => t.ToLowerInvariant() == tag));

        if (isRead is not null)
            result = result.Where(b => b.IsRead == isRead);

        if (keyword is not null)
            result = result.Where(b =>
                b.Title.ToLowerInvariant().Contains(keyword) ||
                (b.Notes != null && b.Notes.ToLowerInvariant().Contains(keyword)));

        return result.ToList();
    }

    public async Task<BookmarkSummaryResponse> GetSummaryAsync()
    {
        var bookmarks = await db.Bookmarks.ToListAsync();
        var tags = bookmarks
            .SelectMany(b => b.Tags)
            .GroupBy(t => t.ToLowerInvariant())
            .Select(g => new TagCount { Tag = g.Key, Count = g.Count() })
            .OrderByDescending(t => t.Count)
            .ThenBy(t => t.Tag)
            .ToList();

        return new BookmarkSummaryResponse
        {
            Total = bookmarks.Count,
            Unread = bookmarks.Count(b => !b.IsRead),
            Tags = tags,
            UntaggedCount = bookmarks.Count(b => b.Tags.Count == 0)
        };
    }
}
