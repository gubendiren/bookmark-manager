using BookmarkManager.Api.Models;

namespace BookmarkManager.Api.Repositories;

public interface IBookmarkRepository
{
    Task<Bookmark> CreateAsync(Bookmark bookmark);
    Task<IEnumerable<Bookmark>> GetAllAsync();
    Task<Bookmark?> GetByIdAsync(Guid id);
    Task<Bookmark?> UpdateAsync(Bookmark bookmark);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsByNormalizedUrlAsync(string normalizedUrl);
    Task<Bookmark?> GetByNormalizedUrlAsync(string normalizedUrl);
    Task<IEnumerable<Bookmark>> GetFilteredAsync(string? tag, bool? isRead, string? keyword);
}
