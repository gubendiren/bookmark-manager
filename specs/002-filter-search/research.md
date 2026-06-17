# Research: Filtering and Search

**Date**: 2026-06-16
**Branch**: `002-filter-search`

---

## Decision 1 — Filter Parameter Transport

**Decision**: Extend the existing `GET /api/bookmarks` endpoint with optional query parameters (`?tag=`, `?status=`, `?q=`).

**Rationale**: Standard REST pattern for filtering read collections. Preserves idempotency and cacheability. ASP.NET Core binds query parameters automatically via `[FromQuery]` and a DTO class — no manual parsing required.

**Alternatives considered**:
- `POST /api/bookmarks/search` with a request body — rejected: non-standard for read operations; breaks HTTP semantics.
- Separate `/api/bookmarks/filter` endpoint — rejected: violates YAGNI; the existing endpoint is the natural extension point.

---

## Decision 2 — Tag Filter Semantics

**Decision**: Exact match, case-insensitive. A bookmark matches if any element of its `Tags` list equals the filter value after both are lowercased.

```
b.Tags.Any(t => t.ToLowerInvariant() == tag.ToLowerInvariant())
```

**Rationale**: Tags are categorical labels with well-defined boundaries. Substring matching would produce false positives — filtering by "js" would incorrectly surface bookmarks tagged "nodejs".

**Empty/whitespace tag**: Treated as no tag filter (per FR-009 / clarification). Normalised to `null` before filtering.

---

## Decision 3 — Keyword Search Implementation

**Decision**: Case-insensitive substring match against `Title` and `Notes` independently. URL is explicitly excluded (per FR-003 / clarification).

```
keyword = keyword.Trim().ToLowerInvariant();
b.Title.ToLowerInvariant().Contains(keyword)
|| (b.Notes != null && b.Notes.ToLowerInvariant().Contains(keyword))
```

**Rationale**: Simple, zero additional dependencies, fully deterministic with EF Core in-memory provider. `Contains` in LINQ translates correctly for in-memory; no server-side translation ambiguity.

**Whitespace-only keyword**: Trimmed; if empty after trim, treated as null (no filter) — per FR-007.

---

## Decision 4 — Read Status Filter Mapping

**Decision**: Three accepted string values; all others are validation errors.

| Value | Predicate applied |
|-------|------------------|
| `"read"` | `b.IsRead == true` |
| `"unread"` | `b.IsRead == false` |
| `"all"` or omitted | No predicate (all bookmarks) |
| Anything else | 400 Bad Request — per FR-008 |

**Rationale**: Explicit allow-list prevents silent mismatches (e.g., "Read" with capital R falling through to "all" unexpectedly). Invalid values fail fast with a clear message.

---

## Decision 5 — Repository Strategy

**Decision**: Add a new `GetFilteredAsync(string? tag, bool? isRead, string? keyword)` method to `IBookmarkRepository`. Existing `GetAllAsync()` is preserved unchanged to avoid breaking existing unit tests.

The service calls `GetFilteredAsync` for the `GET /api/bookmarks` path. All three filter parameters are nullable; a null value for any parameter means "no restriction on that dimension."

**Rationale**: Additive change — no existing test needs to change. Keeps `GetAllAsync` for any future callers that genuinely want an unrestricted fetch.

---

## Decision 6 — Frontend Filter UX

**Decision**: New `BookmarkFilter` component rendered above `BookmarkList` in `App.jsx`. Contains:
- Text input for **tag** — debounced 300 ms
- `<select>` for **read status** — immediate (no debounce needed; discrete options)
- Text input for **keyword** — debounced 300 ms

Filter state is held in `App.jsx` and passed down as props. On filter change, `BookmarkList` re-fetches using `bookmarkService.getAll(filter)` which appends non-empty params as query-string values.

**Rationale**: Debouncing prevents a network request on every keystroke for free-text inputs. Status is a dropdown with discrete values so no debounce is needed. Lifting state to `App.jsx` keeps `BookmarkList` stateless with respect to filter criteria.

**Alternatives considered**:
- Separate "Apply" button — rejected: adds friction; debounce achieves the same throttling goal with better UX.
- Filter state in URL — rejected: stateless per spec assumption; adds complexity.

---

## Decision 7 — No New Entities or Migrations

**Decision**: No database schema changes required. `BookmarkFilterRequest` is a transient value object (not persisted).

**Rationale**: All filter dimensions operate on fields that already exist on the `Bookmark` entity (`Tags`, `IsRead`, `Title`, `Notes`). The in-memory database resets on restart anyway — no migration concern.
