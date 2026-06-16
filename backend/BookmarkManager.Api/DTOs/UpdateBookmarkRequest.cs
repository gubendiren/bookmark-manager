using BookmarkManager.Api.Validation;

namespace BookmarkManager.Api.DTOs;

public class UpdateBookmarkRequest
{
    public string? Url { get; set; }
    [TrimmedNonWhitespace]
    public string? Title { get; set; }
    public string[]? Tags { get; set; }
    public string? Notes { get; set; }
    public bool? IsRead { get; set; }
}
