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

        _repo.GetAllAsync().Returns(bookmarks);

        var result = await _sut.GetAllAsync();

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
