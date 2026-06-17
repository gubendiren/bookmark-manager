using BookmarkManager.Api.DTOs;
using BookmarkManager.Api.Exceptions;
using BookmarkManager.Api.Models;
using BookmarkManager.Api.Repositories;
using BookmarkManager.Api.Services;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BookmarkManager.Api.Tests.Services;

public class BookmarkServiceTests
{
    private readonly IBookmarkRepository _repo = Substitute.For<IBookmarkRepository>();
    private readonly BookmarkService _sut;

    public BookmarkServiceTests()
    {
        _sut = new BookmarkService(_repo);
    }

    // ── CreateAsync happy path ──────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsBookmarkResponse()
    {
        var request = new CreateBookmarkRequest
        {
            Url = "https://example.com",
            Title = "Example",
            Tags = ["react", "tech"],
            Notes = "A note",
            IsRead = false,
        };

        _repo.ExistsByNormalizedUrlAsync(Arg.Any<string>()).Returns(false);
        _repo.CreateAsync(Arg.Any<Bookmark>()).Returns(ci =>
        {
            var b = ci.Arg<Bookmark>();
            b.Id = Guid.NewGuid();
            return b;
        });

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.Url.Should().Be("https://example.com");
        result.Title.Should().Be("Example");
        result.Tags.Should().BeEquivalentTo(["react", "tech"]);
        result.Notes.Should().Be("A note");
        result.IsRead.Should().BeFalse();
    }

    // ── CreateAsync duplicate URL ───────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateUrl_ThrowsDuplicateUrlExceptionWithConflictingTitle()
    {
        var existing = new Bookmark { Id = Guid.NewGuid(), Title = "React Docs", Url = "https://react.dev" };

        _repo.ExistsByNormalizedUrlAsync(Arg.Any<string>()).Returns(true);
        _repo.GetByNormalizedUrlAsync(Arg.Any<string>()).Returns(existing);

        var request = new CreateBookmarkRequest { Url = "https://react.dev", Title = "Another" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<DuplicateUrlException>()
            .WithMessage("*React Docs*");
    }

    // ── CreateAsync validation errors ──────────────────────────────────────

    [Fact]
    public async Task CreateAsync_MissingUrl_ThrowsArgumentException()
    {
        var request = new CreateBookmarkRequest { Url = "", Title = "Title" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_MissingTitle_ThrowsArgumentException()
    {
        var request = new CreateBookmarkRequest { Url = "https://example.com", Title = "" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_InvalidUrlScheme_ThrowsArgumentException()
    {
        var request = new CreateBookmarkRequest { Url = "ftp://example.com", Title = "Title" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── GetAllAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllBookmarks()
    {
        var bookmarks = new List<Bookmark>
        {
            new() { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", Tags = [], CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Url = "https://b.com", Title = "B", Tags = [], CreatedAt = DateTime.UtcNow },
        };

        _repo.GetFilteredAsync(null, null, null).Returns(bookmarks);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest());

        result.Should().HaveCount(2);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsBookmark()
    {
        var id = Guid.NewGuid();
        var bookmark = new Bookmark { Id = id, Url = "https://a.com", Title = "A", Tags = [], CreatedAt = DateTime.UtcNow };

        _repo.GetByIdAsync(id).Returns(bookmark);

        var result = await _sut.GetByIdAsync(id);

        result.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Bookmark?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PartialUpdate_PreservesUntouchedFields()
    {
        var id = Guid.NewGuid();
        var existing = new Bookmark
        {
            Id = id, Url = "https://a.com", Title = "Original", Tags = ["tag1"],
            Notes = "Old note", IsRead = false, CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        };

        _repo.GetByIdAsync(id).Returns(existing);
        _repo.ExistsByNormalizedUrlAsync(Arg.Any<string>()).Returns(false);
        _repo.UpdateAsync(Arg.Any<Bookmark>()).Returns(ci => ci.Arg<Bookmark>());

        var request = new UpdateBookmarkRequest { IsRead = true };
        var result = await _sut.UpdateAsync(id, request);

        result.IsRead.Should().BeTrue();
        result.Title.Should().Be("Original");
        result.Tags.Should().BeEquivalentTo(["tag1"]);
        result.Notes.Should().Be("Old note");
    }

    [Fact]
    public async Task UpdateAsync_NullTags_LeavesTagsUnchanged()
    {
        var id = Guid.NewGuid();
        var existing = new Bookmark
        {
            Id = id, Url = "https://a.com", Title = "T", Tags = ["keep"],
            CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        };

        _repo.GetByIdAsync(id).Returns(existing);
        _repo.UpdateAsync(Arg.Any<Bookmark>()).Returns(ci => ci.Arg<Bookmark>());

        var result = await _sut.UpdateAsync(id, new UpdateBookmarkRequest { Tags = null });

        result.Tags.Should().BeEquivalentTo(["keep"]);
    }

    [Fact]
    public async Task UpdateAsync_EmptyTags_ClearsAllTags()
    {
        var id = Guid.NewGuid();
        var existing = new Bookmark
        {
            Id = id, Url = "https://a.com", Title = "T", Tags = ["keep"],
            CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        };

        _repo.GetByIdAsync(id).Returns(existing);
        _repo.UpdateAsync(Arg.Any<Bookmark>()).Returns(ci => ci.Arg<Bookmark>());

        var result = await _sut.UpdateAsync(id, new UpdateBookmarkRequest { Tags = [] });

        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_SameUrl_IsAccepted()
    {
        var id = Guid.NewGuid();
        var existing = new Bookmark
        {
            Id = id, Url = "https://a.com", Title = "T", Tags = [],
            CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        };

        _repo.GetByIdAsync(id).Returns(existing);
        _repo.GetByNormalizedUrlAsync("https://a.com").Returns(existing);
        _repo.UpdateAsync(Arg.Any<Bookmark>()).Returns(ci => ci.Arg<Bookmark>());

        var result = await _sut.UpdateAsync(id, new UpdateBookmarkRequest { Url = "https://a.com" });

        result.Url.Should().Be("https://a.com");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateUrlOfOtherBookmark_ThrowsDuplicateUrlException()
    {
        var id = Guid.NewGuid();
        var conflicting = new Bookmark { Id = Guid.NewGuid(), Url = "https://b.com", Title = "Other" };
        var existing = new Bookmark
        {
            Id = id, Url = "https://a.com", Title = "T", Tags = [],
            CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        };

        _repo.GetByIdAsync(id).Returns(existing);
        _repo.GetByNormalizedUrlAsync("https://b.com").Returns(conflicting);

        var act = () => _sut.UpdateAsync(id, new UpdateBookmarkRequest { Url = "https://b.com" });

        await act.Should().ThrowAsync<DuplicateUrlException>().WithMessage("*Other*");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Bookmark?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateBookmarkRequest());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── GetAllAsync filter — tag ────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WithTagFilter_ReturnsOnlyMatchingBookmarks()
    {
        var reactBookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://react.dev", Title = "React", Tags = ["react"], CreatedAt = DateTime.UtcNow };
        var tsBookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://ts.dev", Title = "TS", Tags = ["typescript"], CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync("react", null, null).Returns([reactBookmark]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Tag = "react" });

        result.Should().HaveCount(1);
        result.Single().Title.Should().Be("React");
    }

    [Fact]
    public async Task GetAllAsync_WithTagFilterCaseInsensitive_ReturnsMatch()
    {
        var reactBookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://react.dev", Title = "React", Tags = ["react"], CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync("react", null, null).Returns([reactBookmark]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Tag = "REACT" });

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyTagFilter_ReturnsAll()
    {
        var bookmarks = new List<Bookmark>
        {
            new() { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", Tags = ["a"], CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Url = "https://b.com", Title = "B", Tags = ["b"], CreatedAt = DateTime.UtcNow },
        };

        _repo.GetFilteredAsync(null, null, null).Returns(bookmarks);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Tag = "" });

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithTagFilter_NoMatches_ReturnsEmptyList()
    {
        _repo.GetFilteredAsync("python", null, null).Returns([]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Tag = "python" });

        result.Should().BeEmpty();
    }

    // ── GetAllAsync filter — read status ────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WithStatusUnread_ReturnsOnlyUnread()
    {
        var unread = new Bookmark { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", IsRead = false, Tags = [], CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync(null, false, null).Returns([unread]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Status = "unread" });

        result.Should().HaveCount(1);
        result.Single().IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithStatusRead_ReturnsOnlyRead()
    {
        var read = new Bookmark { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", IsRead = true, Tags = [], CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync(null, true, null).Returns([read]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Status = "read" });

        result.Should().HaveCount(1);
        result.Single().IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WithStatusAll_ReturnsAll()
    {
        var bookmarks = new List<Bookmark>
        {
            new() { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", IsRead = true, Tags = [], CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Url = "https://b.com", Title = "B", IsRead = false, Tags = [], CreatedAt = DateTime.UtcNow },
        };

        _repo.GetFilteredAsync(null, null, null).Returns(bookmarks);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Status = "all" });

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusOmitted_ReturnsAll()
    {
        var bookmarks = new List<Bookmark>
        {
            new() { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", Tags = [], CreatedAt = DateTime.UtcNow },
        };

        _repo.GetFilteredAsync(null, null, null).Returns(bookmarks);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest());

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithInvalidStatus_ThrowsArgumentException()
    {
        var act = () => _sut.GetAllAsync(new BookmarkFilterRequest { Status = "badvalue" });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*badvalue*");
    }

    [Fact]
    public async Task GetAllAsync_WithStatusUnread_NoMatches_ReturnsEmptyList()
    {
        _repo.GetFilteredAsync(null, false, null).Returns([]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Status = "unread" });

        result.Should().BeEmpty();
    }

    // ── GetAllAsync filter — keyword ────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WithKeyword_MatchesTitle()
    {
        var bookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://a.com", Title = "React Hooks Guide", Tags = [], CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync(null, null, "hook").Returns([bookmark]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Keyword = "hook" });

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithKeyword_MatchesNotes()
    {
        var bookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://a.com", Title = "TS Handbook", Notes = "Good reference for generics.", Tags = [], CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync(null, null, "generics").Returns([bookmark]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Keyword = "generics" });

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithKeyword_CaseInsensitiveMatch()
    {
        var bookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://a.com", Title = "React Hooks Guide", Tags = [], CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync(null, null, "hook").Returns([bookmark]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Keyword = "HOOK" });

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithWhitespaceKeyword_ReturnsAll()
    {
        var bookmarks = new List<Bookmark>
        {
            new() { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", Tags = [], CreatedAt = DateTime.UtcNow },
        };

        _repo.GetFilteredAsync(null, null, null).Returns(bookmarks);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Keyword = "   " });

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithKeyword_NoMatches_ReturnsEmptyList()
    {
        _repo.GetFilteredAsync(null, null, "xyzzy").Returns([]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Keyword = "xyzzy" });

        result.Should().BeEmpty();
    }

    // ── GetAllAsync filter — combined ───────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WithTagAndStatus_ReturnsIntersection()
    {
        var bookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://a.com", Title = "React Hooks", Tags = ["react"], IsRead = false, CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync("react", false, null).Returns([bookmark]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Tag = "react", Status = "unread" });

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithAllThreeFilters_ReturnsIntersection()
    {
        var bookmark = new Bookmark { Id = Guid.NewGuid(), Url = "https://a.com", Title = "React Hooks Guide", Tags = ["react"], IsRead = false, CreatedAt = DateTime.UtcNow };

        _repo.GetFilteredAsync("react", false, "hooks").Returns([bookmark]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Tag = "react", Status = "unread", Keyword = "hooks" });

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_WithAllThreeFilters_NoMatches_ReturnsEmptyList()
    {
        _repo.GetFilteredAsync("react", false, "typescript").Returns([]);

        var result = await _sut.GetAllAsync(new BookmarkFilterRequest { Tag = "react", Status = "unread", Keyword = "typescript" });

        result.Should().BeEmpty();
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_Succeeds()
    {
        _repo.DeleteAsync(Arg.Any<Guid>()).Returns(true);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _repo.DeleteAsync(Arg.Any<Guid>()).Returns(false);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
