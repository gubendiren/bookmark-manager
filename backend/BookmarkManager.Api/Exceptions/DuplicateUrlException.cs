namespace BookmarkManager.Api.Exceptions;

public class DuplicateUrlException(string conflictingTitle, Guid conflictingId)
    : Exception($"This URL is already saved as \"{conflictingTitle}\".")
{
    public string ConflictingTitle { get; } = conflictingTitle;
    public Guid ConflictingId { get; } = conflictingId;
}
