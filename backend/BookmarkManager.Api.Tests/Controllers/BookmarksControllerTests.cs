using System.Net;
using System.Net.Http.Json;
using BookmarkManager.Api.DTOs;
using BookmarkManager.Api.Exceptions;
using BookmarkManager.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BookmarkManager.Api.Tests.Controllers;

public class BookmarksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BookmarksControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithService(IBookmarkService service)
    {
        return _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddScoped<IBookmarkService>(_ => service)))
            .CreateClient();
    }

    // ── POST /api/bookmarks ─────────────────────────────────────────────────

    [Fact]
    public async Task Post_ValidRequest_Returns201WithLocationHeader()
    {
        var id = Guid.NewGuid();
        var service = Substitute.For<IBookmarkService>();
        service.CreateAsync(Arg.Any<CreateBookmarkRequest>()).Returns(new BookmarkResponse
        {
            Id = id, Url = "https://example.com", Title = "Example",
            Tags = [], CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        });

        var client = CreateClientWithService(service);

        var response = await client.PostAsJsonAsync("/api/bookmarks",
            new { url = "https://example.com", title = "Example" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(id.ToString());
    }

    [Fact]
    public async Task Post_DuplicateUrl_Returns409()
    {
        var service = Substitute.For<IBookmarkService>();
        service.CreateAsync(Arg.Any<CreateBookmarkRequest>())
            .ThrowsAsync(new DuplicateUrlException("React Docs", Guid.NewGuid()));

        var client = CreateClientWithService(service);

        var response = await client.PostAsJsonAsync("/api/bookmarks",
            new { url = "https://react.dev", title = "React" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("React Docs");
    }

    [Fact]
    public async Task Post_InvalidRequest_Returns400()
    {
        var service = Substitute.For<IBookmarkService>();
        service.CreateAsync(Arg.Any<CreateBookmarkRequest>())
            .ThrowsAsync(new ArgumentException("URL is required.", "url"));

        var client = CreateClientWithService(service);

        var response = await client.PostAsJsonAsync("/api/bookmarks",
            new { url = "", title = "Title" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/bookmarks ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns200WithArray()
    {
        var service = Substitute.For<IBookmarkService>();
        service.GetAllAsync().Returns([
            new BookmarkResponse { Id = Guid.NewGuid(), Url = "https://a.com", Title = "A", Tags = [], CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow },
        ]);

        var client = CreateClientWithService(service);

        var response = await client.GetAsync("/api/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<BookmarkResponse[]>();
        items.Should().HaveCount(1);
    }

    // ── GET /api/bookmarks/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        var id = Guid.NewGuid();
        var service = Substitute.For<IBookmarkService>();
        service.GetByIdAsync(id).Returns(new BookmarkResponse
        {
            Id = id, Url = "https://a.com", Title = "A", Tags = [], CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        });

        var client = CreateClientWithService(service);

        var response = await client.GetAsync($"/api/bookmarks/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var service = Substitute.For<IBookmarkService>();
        service.GetByIdAsync(Arg.Any<Guid>()).ThrowsAsync(new NotFoundException("Not found"));

        var client = CreateClientWithService(service);

        var response = await client.GetAsync($"/api/bookmarks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PATCH /api/bookmarks/{id} ───────────────────────────────────────────

    [Fact]
    public async Task Patch_ValidUpdate_Returns200()
    {
        var id = Guid.NewGuid();
        var service = Substitute.For<IBookmarkService>();
        service.UpdateAsync(id, Arg.Any<UpdateBookmarkRequest>()).Returns(new BookmarkResponse
        {
            Id = id, Url = "https://a.com", Title = "Updated", Tags = [], CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow,
        });

        var client = CreateClientWithService(service);

        var response = await client.PatchAsJsonAsync($"/api/bookmarks/{id}",
            new { title = "Updated" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Patch_NotFound_Returns404()
    {
        var service = Substitute.For<IBookmarkService>();
        service.UpdateAsync(Arg.Any<Guid>(), Arg.Any<UpdateBookmarkRequest>())
            .ThrowsAsync(new NotFoundException("Not found"));

        var client = CreateClientWithService(service);

        var response = await client.PatchAsJsonAsync($"/api/bookmarks/{Guid.NewGuid()}",
            new { title = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Patch_DuplicateUrl_Returns409()
    {
        var service = Substitute.For<IBookmarkService>();
        service.UpdateAsync(Arg.Any<Guid>(), Arg.Any<UpdateBookmarkRequest>())
            .ThrowsAsync(new DuplicateUrlException("Other Bookmark", Guid.NewGuid()));

        var client = CreateClientWithService(service);

        var response = await client.PatchAsJsonAsync($"/api/bookmarks/{Guid.NewGuid()}",
            new { url = "https://other.com" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── DELETE /api/bookmarks/{id} ──────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        var id = Guid.NewGuid();
        var service = Substitute.For<IBookmarkService>();
        service.DeleteAsync(id).Returns(Task.CompletedTask);

        var client = CreateClientWithService(service);

        var response = await client.DeleteAsync($"/api/bookmarks/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var service = Substitute.For<IBookmarkService>();
        service.DeleteAsync(Arg.Any<Guid>()).ThrowsAsync(new NotFoundException("Not found"));

        var client = CreateClientWithService(service);

        var response = await client.DeleteAsync($"/api/bookmarks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/bookmarks/summary ──────────────────────────────────────────

    [Fact]
    public async Task GetSummary_Returns200WithCorrectShape()
    {
        var service = Substitute.For<IBookmarkService>();
        service.GetSummaryAsync().Returns(new BookmarkSummaryResponse
        {
            Total = 10,
            Unread = 4,
            Tags = [new TagCount { Tag = "react", Count = 2 }, new TagCount { Tag = "hooks", Count = 1 }],
            UntaggedCount = 1,
        });

        var client = CreateClientWithService(service);
        var response = await client.GetAsync("/api/bookmarks/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<BookmarkSummaryResponse>();
        content.Should().NotBeNull();
        content!.Total.Should().Be(10);
        content.Unread.Should().Be(4);
        content.Tags.Should().HaveCount(2);
        content.UntaggedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_EmptyCollection_Returns200WithZeroCounts()
    {
        var service = Substitute.For<IBookmarkService>();
        service.GetSummaryAsync().Returns(new BookmarkSummaryResponse
        {
            Total = 0,
            Unread = 0,
            Tags = [],
            UntaggedCount = 0,
        });

        var client = CreateClientWithService(service);
        var response = await client.GetAsync("/api/bookmarks/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<BookmarkSummaryResponse>();
        content!.Total.Should().Be(0);
        content.Tags.Should().BeEmpty();
        content.UntaggedCount.Should().Be(0);
    }
}
