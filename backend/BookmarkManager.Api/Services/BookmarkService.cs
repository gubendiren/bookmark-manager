using System.Text.RegularExpressions;
using BookmarkManager.Api.DTOs;
using BookmarkManager.Api.Exceptions;
using BookmarkManager.Api.Models;
using BookmarkManager.Api.Repositories;

namespace BookmarkManager.Api.Services;

public partial class BookmarkService(IBookmarkRepository repository) : IBookmarkService
{
    [GeneratedRegex(@"^https?://", RegexOptions.IgnoreCase)]
    private static partial Regex HttpSchemeRegex();

    public async Task<BookmarkResponse> CreateAsync(CreateBookmarkRequest request)
    {
        ValidateUrl(request.Url, nameof(request.Url));
        ValidateTitle(request.Title, nameof(request.Title));

        var normalizedUrl = Normalize(request.Url);
        var existing = await repository.GetByNormalizedUrlAsync(normalizedUrl);
        if (existing is not null)
            throw new DuplicateUrlException(existing.Title, existing.Id);

        var bookmark = new Bookmark
        {
            Url = request.Url.Trim(),
            Title = request.Title.Trim(),
            Tags = DeduplicateTags(request.Tags ?? []),
            Notes = NormalizeNotes(request.Notes),
            IsRead = request.IsRead ?? false,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
        };

        var created = await repository.CreateAsync(bookmark);
        return ToResponse(created);
    }

    public async Task<IEnumerable<BookmarkResponse>> GetAllAsync(BookmarkFilterRequest filter)
    {
        var tag = string.IsNullOrWhiteSpace(filter.Tag) ? null : filter.Tag.Trim().ToLowerInvariant();

        bool? isRead = filter.Status?.ToLowerInvariant() switch
        {
            null or "all" => null,
            "read" => true,
            "unread" => false,
            _ => throw new ArgumentException(
                $"Invalid status value '{filter.Status}'. Accepted values: read, unread, all.",
                nameof(filter.Status)),
        };

        var keyword = string.IsNullOrWhiteSpace(filter.Keyword) ? null : filter.Keyword.Trim().ToLowerInvariant();

        var bookmarks = await repository.GetFilteredAsync(tag, isRead, keyword);
        return bookmarks.Select(ToResponse);
    }

    public async Task<BookmarkResponse> GetByIdAsync(Guid id)
    {
        var bookmark = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Bookmark '{id}' was not found.");
        return ToResponse(bookmark);
    }

    public async Task<BookmarkResponse> UpdateAsync(Guid id, UpdateBookmarkRequest request)
    {
        var bookmark = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Bookmark '{id}' was not found.");

        if (request.Url is not null)
        {
            ValidateUrl(request.Url, nameof(request.Url));
            var normalizedNew = Normalize(request.Url);
            var normalizedCurrent = Normalize(bookmark.Url);
            if (normalizedNew != normalizedCurrent)
            {
                var conflicting = await repository.GetByNormalizedUrlAsync(normalizedNew);
                if (conflicting is not null)
                    throw new DuplicateUrlException(conflicting.Title, conflicting.Id);
            }
            bookmark.Url = request.Url.Trim();
        }

        if (request.Title is not null)
        {
            ValidateTitle(request.Title, nameof(request.Title));
            bookmark.Title = request.Title.Trim();
        }

        if (request.Tags is not null)
            bookmark.Tags = DeduplicateTags(request.Tags);

        if (request.Notes is not null)
            bookmark.Notes = NormalizeNotes(request.Notes);

        if (request.IsRead is not null)
            bookmark.IsRead = request.IsRead.Value;

        bookmark.LastModifiedAt = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(bookmark);
        return ToResponse(updated!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
            throw new NotFoundException($"Bookmark '{id}' was not found.");
    }

    public async Task<BookmarkSummaryResponse> GetSummaryAsync() =>
        await repository.GetSummaryAsync();

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static string Normalize(string url) => url.Trim().ToLowerInvariant();

    private static void ValidateUrl(string url, string paramName)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required.", paramName);
        if (!HttpSchemeRegex().IsMatch(url.Trim()))
            throw new ArgumentException("URL must begin with http:// or https://.", paramName);
    }

    private static void ValidateTitle(string title, string paramName)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", paramName);
    }

    private static List<string> DeduplicateTags(string[] tags) =>
        tags.Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string? NormalizeNotes(string? notes)
    {
        if (notes is null) return null;
        var trimmed = notes.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static BookmarkResponse ToResponse(Bookmark b) => new()
    {
        Id = b.Id,
        Url = b.Url,
        Title = b.Title,
        Tags = [.. b.Tags],
        Notes = b.Notes,
        IsRead = b.IsRead,
        CreatedAt = b.CreatedAt,
        LastModifiedAt = b.LastModifiedAt,
    };
}
