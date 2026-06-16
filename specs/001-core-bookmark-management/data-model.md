# Data Model: Core Bookmark Management

**Date**: 2026-06-16
**Branch**: `001-core-bookmark-management`

---

## Entity: Bookmark

| Field | C# Type | Nullable | Default | Constraints |
|-------|---------|----------|---------|-------------|
| `Id` | `Guid` | No | System-generated | Primary key; UUID v4; assigned at creation |
| `Url` | `string` | No | — | Required; must match `^https?://`; max 2048 chars; stored trimmed |
| `Title` | `string` | No | — | Required; max 500 chars; whitespace-only treated as missing |
| `Tags` | `List<string>` | No | `[]` | Optional; defaults to empty list; duplicates silently removed; each tag max 100 chars |
| `Notes` | `string?` | Yes | `null` | Optional; whitespace-only stored as `null`; max 10 000 chars |
| `IsRead` | `bool` | No | `false` | Binary status; `false` = unread, `true` = read; defaults to unread on creation |
| `CreatedAt` | `DateTime` | No | UTC now | Set at creation; never modified |
| `LastModifiedAt` | `DateTime` | No | UTC now | Set at creation; updated on every successful PATCH |

---

## EF Core Entity Configuration

```csharp
// Bookmark.cs (domain entity — no EF attributes; configured via Fluent API)
public class Bookmark
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string? Notes { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}
```

`Tags` is stored as a JSON column in EF Core 8 (`OwnsMany` or `HasConversion<string>`
with JSON serialization for the In-Memory provider).

---

## DTO Layer

### CreateBookmarkRequest

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `url` | `string` | Yes | Validated for format and uniqueness |
| `title` | `string` | Yes | Whitespace-only rejected |
| `tags` | `string[]` | No | Defaults to `[]`; duplicates removed |
| `notes` | `string?` | No | Whitespace-only stored as `null` |
| `isRead` | `bool?` | No | Defaults to `false` |

### UpdateBookmarkRequest (PATCH)

All fields are optional. `null` = no change, except `tags` (see table below).

| Field | Type | Null behaviour |
|-------|------|----------------|
| `url` | `string?` | null → unchanged |
| `title` | `string?` | null → unchanged |
| `tags` | `string[]?` | null → unchanged; `[]` → clear all tags |
| `notes` | `string?` | null → unchanged; `""` → stored as null (cleared) |
| `isRead` | `bool?` | null → unchanged |

### BookmarkResponse

Returned by all successful create, update, and retrieve operations.

| Field | Type | Notes |
|-------|------|-------|
| `id` | `string` (UUID) | System-assigned identifier |
| `url` | `string` | As stored (trimmed) |
| `title` | `string` | As stored |
| `tags` | `string[]` | Deduplicated, in insertion order |
| `notes` | `string?` | `null` if not set |
| `isRead` | `bool` | Current read/unread status |
| `createdAt` | `string` (ISO 8601) | UTC timestamp |
| `lastModifiedAt` | `string` (ISO 8601) | UTC timestamp |

---

## Normalization Rules

### URL Uniqueness Key

```
normalizedUrl = url.Trim().ToLowerInvariant()
```

- Applied before both storage and duplicate lookup
- URL fragments (`#anchor`) and query strings (`?q=x`) are part of the key
- A PATCH that sets `url` to the same normalized value as the bookmark's current
  `url` is accepted (not treated as a self-duplicate)

### Tag Deduplication

```
tags = submittedTags
         .Select(t => t.Trim())
         .Where(t => t.Length > 0)
         .Distinct(StringComparer.OrdinalIgnoreCase)
         .ToList()
```

Case-insensitive deduplication; empty/whitespace tags discarded; order preserved
(first occurrence wins).

### Whitespace Normalization

- `Url.Trim()` applied before validation and storage
- `Title.Trim()` applied; empty after trim → validation error
- `Notes?.Trim()` → if empty or whitespace after trim, stored as `null`

---

## State Transitions

### IsRead

```
false (unread) ←→ true (read)    [via PATCH /api/bookmarks/{id}]
Initial state: false
```

No intermediate states exist.

### Bookmark Lifecycle

```
[Created] → [Updated (0..n times)] → [Deleted]
```

Deletion is permanent. No soft-delete, archive, or recovery path exists.

---

## Validation Summary (derived from spec FR-001 – FR-010)

| Rule | Source | Error type |
|------|--------|-----------|
| URL required | FR-003 | 400 validation error |
| URL format `^https?://` | FR-010 | 400 validation error |
| URL max 2048 chars | Research Decision 2 | 400 validation error |
| Title required | FR-003 | 400 validation error |
| Title whitespace-only rejected | Research Decision 5 | 400 validation error |
| URL uniqueness (case-insensitive, trimmed) | FR-004, FR-009 | 409 Conflict |
| Duplicate error includes conflicting title | FR-004, FR-006 | 409 Conflict |
| Update target must exist | FR-008 | 404 Not Found |
| Delete target must exist | FR-008 | 404 Not Found |
| Multiple errors returned at once | Research Decision 4 | 400 with all errors |
