# Data Model: Filtering and Search

**Date**: 2026-06-16
**Branch**: `002-filter-search`

---

## Existing Entity: Bookmark (unchanged)

No new fields are added. Filtering operates entirely on the existing entity.

| Field | C# Type | Used by filter |
|-------|---------|---------------|
| `Tags` | `List<string>` | Tag filter — exact case-insensitive element match |
| `IsRead` | `bool` | Read-status filter — maps "read"→true, "unread"→false |
| `Title` | `string` | Keyword search — case-insensitive substring |
| `Notes` | `string?` | Keyword search — case-insensitive substring; null treated as empty |

---

## New Value Object: BookmarkFilterRequest

A transient query parameter object — **not persisted**. All fields are optional; omitting a field means no restriction on that dimension.

| Field | C# Type | Nullable | Validation |
|-------|---------|----------|-----------|
| `Tag` | `string?` | Yes | Empty or whitespace → treated as null (no filter) |
| `Status` | `string?` | Yes | Accepted: `"read"`, `"unread"`, `"all"`, null/omitted → 400 if any other value |
| `Keyword` | `string?` | Yes | Whitespace-only → treated as null (no filter) |

```csharp
// DTOs/BookmarkFilterRequest.cs
public class BookmarkFilterRequest
{
    public string? Tag { get; set; }
    public string? Status { get; set; }
    public string? Keyword { get; set; }
}
```

---

## Filter Normalisation Rules

Applied in `BookmarkService` before delegating to the repository:

| Input | Normalised value |
|-------|-----------------|
| `Tag = ""` or whitespace | `null` (no tag filter) |
| `Tag = "React"` | `"React"` — comparison lowercased at query time |
| `Status = null` or `"all"` | `null` (no status filter) |
| `Status = "read"` | `true` |
| `Status = "unread"` | `false` |
| `Status = anything else` | Throw `ArgumentException` → 400 |
| `Keyword = ""` or whitespace | `null` (no keyword filter) |
| `Keyword = "Hook"` | `"hook"` (lowercased for comparison) |

---

## Repository Filter Contract

The repository accepts three independently nullable parameters. A null value for any parameter means "no restriction":

```csharp
// Added to IBookmarkRepository
Task<IEnumerable<Bookmark>> GetFilteredAsync(
    string? tag,       // normalised to lowercase by service before passing
    bool?   isRead,    // null = no filter
    string? keyword);  // normalised to lowercase by service before passing
```

**LINQ predicate composition** (applied with AND logic):

```text
WHERE
  (tag     IS NULL OR Tags.Any(t => t.ToLowerInvariant() == tag))
  AND (isRead  IS NULL OR IsRead == isRead)
  AND (keyword IS NULL OR Title.ToLowerInvariant().Contains(keyword)
                       OR (Notes != null AND Notes.ToLowerInvariant().Contains(keyword)))
ORDER BY CreatedAt ASC
```

---

## Frontend Filter State

Held in `App.jsx` React state; not persisted to localStorage, URL, or session storage.

```js
const [filter, setFilter] = useState({ tag: '', status: 'all', keyword: '' })
```

| Field | Control | Debounce |
|-------|---------|---------|
| `tag` | Text input | 300 ms |
| `status` | `<select>` (all / read / unread) | None |
| `keyword` | Text input | 300 ms |

When any field changes, `BookmarkList` re-fetches with the active filter. Empty string fields are omitted from the query string.
