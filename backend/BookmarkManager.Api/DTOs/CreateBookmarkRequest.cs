using System.ComponentModel.DataAnnotations;
using BookmarkManager.Api.Validation;

namespace BookmarkManager.Api.DTOs;

public class CreateBookmarkRequest
{
    [Required]
    public string Url { get; set; } = string.Empty;

    [Required]
    [TrimmedNonWhitespace]
    public string Title { get; set; } = string.Empty;

    public string[]? Tags { get; set; }

    public string? Notes { get; set; }

    public bool? IsRead { get; set; }
}
