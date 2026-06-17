using BookmarkManager.Api.DTOs;

namespace BookmarkManager.Api.Services;

public interface IBookmarkService
{
    Task<BookmarkResponse> CreateAsync(CreateBookmarkRequest request);
    Task<IEnumerable<BookmarkResponse>> GetAllAsync();
    Task<BookmarkResponse> GetByIdAsync(Guid id);
    Task<BookmarkResponse> UpdateAsync(Guid id, UpdateBookmarkRequest request);
    Task DeleteAsync(Guid id);
    Task<BookmarkSummaryResponse> GetSummaryAsync();
}
