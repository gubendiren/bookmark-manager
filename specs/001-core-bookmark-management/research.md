# Research: Core Bookmark Management

**Phase**: 0 — Design Decisions
**Date**: 2026-06-16
**Branch**: `001-core-bookmark-management`

All technology choices were supplied by the user. This document records the key design
decisions derived from the spec, constitution, and checklist gap analysis.

---

## Decision 1: PATCH Partial Update Semantics

**Decision**: Use a nullable DTO (`UpdateBookmarkRequest`) where each field is optional.
`null` means "no change"; an explicit value (including `[]` for tags) means "update to
this value."

**Rationale**: PATCH is the correct HTTP verb for partial updates (constitution Principle I).
Nullable fields in a C# DTO are the simplest .NET 8 pattern for this without requiring
`JsonPatchDocument` or custom serialization middleware. The spec (FR-005) explicitly
requires omitted fields to remain unchanged.

**Tag field special case** (resolves CHK007/CHK020):

| Request body for `tags` | Effect |
|-------------------------|--------|
| Field omitted from JSON | Tags unchanged |
| `"tags": null` | Tags unchanged (treated same as omitted) |
| `"tags": []` | All tags cleared |
| `"tags": ["work", "dev"]` | Tags replaced with `["work", "dev"]` (deduplicated) |

**Alternatives considered**:
- `JsonPatchDocument<Bookmark>`: More standards-compliant (RFC 6902) but adds complexity
  for no benefit on a single-user personal tool (Constitution Principle V rejected this).
- Full PUT replacement: Simpler server logic but forces the caller to know all current
  values; incompatible with SC-003 "untouched fields remain unchanged."

---

## Decision 2: URL Normalization for Duplicate Detection

**Decision**: Normalize for comparison only; store as submitted (trimmed).

**Normalization key**: `url.Trim().ToLowerInvariant()`

**Full rules**:
- Leading/trailing whitespace is stripped before storage and before comparison
- Scheme and host are case-insensitive (per RFC 3986); the full URL is lowercased
  for comparison to keep the implementation simple and correct
- URL fragments (`#section`) and query strings (`?q=x`) are included in the
  comparison key — they are part of the URL identity
- A bookmark updating its own URL to the same value (post-normalization) is NOT
  a duplicate of itself (resolves CHK018)

**Rationale**: FR-009 requires case-insensitive, whitespace-trimmed comparison.
Lowercasing the full URL is conservative and safe; it may over-match on
case-sensitive paths (uncommon in practice) but matches typical user expectations.

---

## Decision 3: Duplicate URL Error Response Shape

**Decision**: Return HTTP 409 Conflict with a ProblemDetails body containing a custom
`conflictingBookmark` extension field.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Duplicate URL",
  "status": 409,
  "detail": "This URL is already saved as \"React Docs\".",
  "conflictingBookmark": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "React Docs"
  }
}
```

**Rationale**: FR-004 and FR-006 require the error to include the conflicting bookmark's
title. ProblemDetails (RFC 7807) is the ASP.NET Core standard for structured error
responses and aligns with Constitution Principle I (RESTful API Design). The `detail`
field includes a human-readable sentence; `conflictingBookmark` provides structured
data for programmatic use.

---

## Decision 4: Multiple Validation Errors Returned Together

**Decision**: Return all validation errors in a single 400 response using ASP.NET Core's
built-in `ValidationProblemDetails`, which aggregates all failing fields.

**Rationale**: Resolves CHK021 (spec gap). ASP.NET Core model validation already collects
all errors before returning; no extra work needed. Users see all issues at once rather
than fixing one at a time.

---

## Decision 5: Whitespace-Only Field Handling

**Decision**: Whitespace-only values for `Url` and `Title` are treated as missing
(validation error). Whitespace-only `Notes` is stored as `null`.

**Rationale**: Resolves CHK023 (spec gap). A title of `"   "` provides no usable
information; treating it as missing aligns with FR-003. Notes are optional and
free-form, so whitespace-only notes are normalized to null silently.

---

## Decision 6: GET Endpoints Included (Resolves CHK001)

**Decision**: Include `GET /api/bookmarks` (list all) and `GET /api/bookmarks/{id}`
(single bookmark) in the API surface for this feature.

**Rationale**: The spec's update and delete user stories both require retrieving
bookmarks ("confirming changes are reflected on retrieval"). The frontend cannot
display bookmarks without a list endpoint. These are implied by the feature scope
and were flagged as missing in CHK001. Adding them here is consistent with
Constitution Principle V (implement what is needed, no more).

---

## Decision 7: Response Payload (Resolves CHK002)

**Decision**: All successful create, update, and retrieve operations return a full
`BookmarkResponse` object. DELETE returns HTTP 204 No Content with no body.

**BookmarkResponse fields**: `id`, `url`, `title`, `tags`, `notes`, `isRead`,
`createdAt`, `lastModifiedAt`

**Rationale**: Returning the full object on create and update allows the client to
confirm exactly what was saved, including system-generated fields (`id`, `createdAt`,
`lastModifiedAt`), without requiring a second GET request.

---

## Decision 8: EF Core In-Memory Registration

**Decision**: Register `BookmarkDbContext` with `UseInMemoryDatabase("BookmarkDb")` in
`Program.cs`. Register `IBookmarkRepository` → `BookmarkRepository` and
`IBookmarkService` → `BookmarkService` as scoped dependencies.

**Rationale**: Standard ASP.NET Core 8 DI pattern. Keeps constitution Principle III
(layered architecture) intact. The in-memory database is ephemeral by design and
documented as an accepted constraint in the spec Assumptions.
