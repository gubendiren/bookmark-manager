using BookmarkManager.Api.DTOs;
using BookmarkManager.Api.Exceptions;
using BookmarkManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookmarkManager.Api.Controllers;

[ApiController]
[Route("api/bookmarks")]
public class BookmarksController(IBookmarkService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookmarkRequest request)
    {
        try
        {
            var result = await service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { [ex.ParamName ?? "request"] = [ex.Message] }));
        }
        catch (DuplicateUrlException ex)
        {
            return Conflict(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "Duplicate URL",
                status = 409,
                detail = ex.Message,
                conflictingBookmark = new { id = ex.ConflictingId, title = ex.ConflictingTitle },
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await service.GetByIdAsync(id);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return Problem(ex.Message, statusCode: 404);
        }
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookmarkRequest request)
    {
        try
        {
            var result = await service.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { [ex.ParamName ?? "request"] = [ex.Message] }));
        }
        catch (NotFoundException ex)
        {
            return Problem(ex.Message, statusCode: 404);
        }
        catch (DuplicateUrlException ex)
        {
            return Conflict(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "Duplicate URL",
                status = 409,
                detail = ex.Message,
                conflictingBookmark = new { id = ex.ConflictingId, title = ex.ConflictingTitle },
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await service.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return Problem(ex.Message, statusCode: 404);
        }
    }
}
