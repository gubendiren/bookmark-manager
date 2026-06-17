# Data Model: Summary Dashboard

**Date**: 2026-06-17
**Branch**: `003-summary-dashboard`

---

## Existing Entity: Bookmark (unchanged)

No fields are added or modified. The summary is computed from existing fields.

| Field | C# Type | Used by summary |
|-------|---------|----------------|
| `Tags` | `List<string>` | Tag breakdown — group and count by tag; empty list → UntaggedCount |
| `IsRead` | `bool` | Unread count — `IsRead == false` |

---

## New Value Object: TagCount

A transient result item — **not persisted**.

| Field | C# Type | Description |
|-------|---------|-------------|
| `Tag` | `string` | The tag label (case-sensitive, as stored on the Bookmark) |
| `Count` | `int` | Number of bookmarks assigned to this tag |

```csharp
// DTOs/TagCount.cs
public record TagCount(string Tag, int Count);
```

---

## New Response DTO: BookmarkSummaryResponse

A transient read-only aggregate — **not persisted**.

| Field | C# Type | Description |
|-------|---------|-------------|
| `Total` | `int` | Total number of bookmarks stored |
| `Unread` | `int` | Number of bookmarks where `IsRead == false` |
| `Tags` | `List<TagCount>` | Per-tag breakdown, sorted alphabetically by tag name |
| `UntaggedCount` | `int` | Number of bookmarks with no tags assigned |

```csharp
// DTOs/BookmarkSummaryResponse.cs
public record BookmarkSummaryResponse(
    int Total,
    int Unread,
    List<TagCount> Tags,
    int UntaggedCount);
```

---

## Aggregation Rules

Applied in `BookmarkRepository.GetSummaryAsync()`:

```text
Total         = COUNT(all bookmarks)
Unread        = COUNT(bookmarks WHERE IsRead == false)
UntaggedCount = COUNT(bookmarks WHERE Tags.Count == 0)
Tags          = for each bookmark b:
                  for each tag t in b.Tags:
                    tagCounts[t] += 1
                → return as List<TagCount> sorted by Tag ASC
```

Key rules:
- A bookmark with zero tags contributes to `UntaggedCount` only (never appears in `Tags`).
- A bookmark with N tags contributes 1 to each of N tag counts and 0 to `UntaggedCount`.
- Tags are grouped by exact string equality (case-sensitive).
- The `Tags` list is sorted alphabetically (ascending) by tag name.

```csharp
// Pseudocode for LINQ aggregation in BookmarkRepository
var all = await _context.Bookmarks.ToListAsync();

var total = all.Count;
var unread = all.Count(b => !b.IsRead);
var untaggedCount = all.Count(b => b.Tags.Count == 0);
var tags = all
    .SelectMany(b => b.Tags)
    .GroupBy(t => t)
    .Select(g => new TagCount(g.Key, g.Count()))
    .OrderBy(tc => tc.Tag)
    .ToList();

return new BookmarkSummaryResponse(total, unread, tags, untaggedCount);
```

---

## Frontend State

No new persisted state. The summary is stored as component-local state inside `BookmarkSummary`:

```js
const [summary, setSummary] = useState({ total: 0, unread: 0, tags: [], untaggedCount: 0 })
```

Fetched via `bookmarkService.getSummary()` on mount and on every `refresh` prop change.

---

## App.jsx Mutation Callbacks

The `refresh` counter is incremented by all three mutation types, triggering both `BookmarkList` and `BookmarkSummary` to re-fetch:

| Event | Handler in App.jsx | Propagated via |
|-------|--------------------|----------------|
| Bookmark created | `handleCreated()` (existing) | `BookmarkForm.onCreated` |
| Bookmark updated | `handleUpdated()` (new) | `BookmarkList` → `BookmarkCard.onUpdated` |
| Bookmark deleted | `handleDeleted()` (new) | `BookmarkList` → `BookmarkCard.onDeleted` |

All three handlers call `setRefresh(r => r + 1)`.
