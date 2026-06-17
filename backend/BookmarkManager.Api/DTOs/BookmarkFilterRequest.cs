using Microsoft.AspNetCore.Mvc;

namespace BookmarkManager.Api.DTOs;

public class BookmarkFilterRequest
{
    public string? Tag { get; set; }
    public string? Status { get; set; }

    [FromQuery(Name = "q")]
    public string? Keyword { get; set; }
}
