# Research: Summary Dashboard

**Date**: 2026-06-17
**Branch**: `003-summary-dashboard`

---

## Decision 1 — Server-Side Endpoint vs. Client-Side Aggregation

**Decision**: Compute the summary server-side via a new `GET /api/bookmarks/summary` endpoint.

**Rationale**: The bookmark list the user sees may be filtered (tag, status, keyword from feature 002). Aggregating from the filtered list in the browser would produce counts reflecting only the visible subset, not the full collection. A dedicated server-side endpoint always aggregates over all bookmarks regardless of any active filter.

**Alternatives considered**:
- Client-side aggregation from `GET /api/bookmarks` response — rejected: produces incorrect counts when filters are active (feature 003 co-exists with 002 filtering).
- Augmenting `GET /api/bookmarks` response with summary metadata — rejected: pollutes the list contract and breaks the single-responsibility pattern of the existing endpoint.

---

## Decision 2 — Endpoint Route

**Decision**: `GET /api/bookmarks/summary`

**Rationale**: The summary is a derived read-only view of the bookmark collection — a natural sub-resource. The existing route `GET /api/bookmarks/{id:guid}` uses a GUID route constraint, so the literal segment "summary" will never match it. No routing conflict exists.

**Alternatives considered**:
- `GET /api/summary` — rejected: resource naming convention in this project roots everything under `/api/bookmarks`; a sibling root is unmotivated.
- `GET /api/bookmarks?summary=true` — rejected: query-parameter-as-action is a REST anti-pattern.

---

## Decision 3 — Aggregation Location in the Stack

**Decision**: Aggregation logic lives in `BookmarkRepository.GetSummaryAsync()`. The repository fetches all bookmarks and groups/counts them in-process.

**Rationale**: Consistent with Principle II (Repository Pattern). The service layer receives the aggregate and maps it to a `BookmarkSummaryResponse` DTO without needing to know persistence details. The controller only handles HTTP concerns.

**Alternatives considered**:
- Aggregation in the service layer — rejected: would require the service to iterate raw `Bookmark` entities, importing knowledge that belongs in the repository.

---

## Decision 4 — Tag Aggregation Rules

**Decision**:
- A bookmark with N tags contributes 1 to the count of each of those N tags.
- A bookmark with zero tags contributes 1 to `UntaggedCount` only.
- Tags are grouped with case-sensitive exact matching (consistent with how tags are stored on the `Bookmark` model).
- Tag counts are returned sorted alphabetically by tag name.

**Rationale**: Matches FR-004 (count once per tag) and FR-005 (separate untagged count). Alphabetical sort makes the breakdown predictable for the user. Case-sensitive matching aligns with the constitution assumption in spec.md and the existing model.

**Alternatives considered**:
- Case-insensitive grouping — rejected: spec states tags are case-sensitive strings; merging "React" and "react" would change semantics established in feature 001.

---

## Decision 5 — Live Sync Mechanism

**Decision**: React state-driven re-fetch using the existing `refresh` integer counter in `App.jsx`. Extend `App.jsx` to also increment `refresh` on bookmark update and delete (currently it only increments on create). `BookmarkSummary` accepts `refresh` as a prop and re-fetches from `/api/bookmarks/summary` whenever the value changes.

**Rationale**: Zero-configuration reactivity with no new dependencies. The `refresh` pattern is already established in this codebase (`BookmarkList` uses it). No WebSocket or polling infrastructure is needed for a single-user, in-session app.

**Alternatives considered**:
- Polling every N seconds — rejected: unnecessary complexity for single-user; adds continuous background network requests.
- Shared state (React Context / Zustand) — rejected: YAGNI — the `refresh` prop pattern achieves the requirement with no new abstractions.
- WebSockets / SSE — rejected: out of scope per spec assumption; cross-device real-time sync is explicitly excluded.

---

## Decision 6 — App.jsx Mutation Callbacks

**Decision**: Add `handleUpdated` and `handleDeleted` functions in `App.jsx`, both incrementing the same `refresh` counter. Thread these down via props: `App` → `BookmarkList` → `BookmarkCard`.

**Rationale**: `BookmarkCard` performs updates and deletes but currently does not notify `App.jsx`. Lifting callbacks to `App.jsx` and threading through `BookmarkList` is the minimal, idiomatic React change. Both `BookmarkList` and `BookmarkSummary` receive the same `refresh` prop, so both re-fetch together after any mutation.

---

## Decision 7 — No New Entities or Migrations

**Decision**: No database schema changes. `BookmarkSummaryResponse` and `TagCount` are transient response DTOs — not persisted.

**Rationale**: The summary is computed entirely from existing `Bookmark` fields (`Tags`, `IsRead`). No new EF Core model or migration is required.
