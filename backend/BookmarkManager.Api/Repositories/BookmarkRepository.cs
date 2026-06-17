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

    public async Task<BookmarkSummaryResponse> GetSummaryAsync()
    {
        var all = await db.Bookmarks.ToListAsync();

        var total = all.Count;
        var unread = all.Count(b => !b.IsRead);
        var untaggedCount = all.Count(b => b.Tags.Count == 0);
        var tags = all
            .SelectMany(b => b.Tags)
            .GroupBy(t => t)
            .Select(g => new TagCount { Tag = g.Key, Count = g.Count() })
            .OrderBy(tc => tc.Tag)
            .ToList();

        return new BookmarkSummaryResponse
        {
            Total = total,
            Unread = unread,
            Tags = tags,
            UntaggedCount = untaggedCount,
        };
    }
}
