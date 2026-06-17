namespace BookmarkManager.Api.DTOs;

public class BookmarkSummaryResponse
{
    public int Total { get; set; }
    public int Unread { get; set; }
    public List<TagCount> Tags { get; set; } = [];
    public int UntaggedCount { get; set; }
}
