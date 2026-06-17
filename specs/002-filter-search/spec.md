# Feature Specification: Filtering and Search

**Feature Branch**: `002-filter-search`

**Created**: 2026-06-16

**Status**: Draft

**Input**: User description: "Filtering and Search: A user can filter by tag, read status, or text search across title and notes. Filters can be combined. No results returns an empty list, not an error."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Filter Bookmarks by Tag (Priority: P1)

A user wants to view only bookmarks that share a specific tag. They select a tag and the list updates to show only matching bookmarks. If no bookmarks carry that tag, an empty list is shown.

**Why this priority**: Tag-based filtering is the primary organisation mechanism in the app and delivers the most immediate value for navigating a growing bookmark collection.

**Independent Test**: Can be fully tested by requesting bookmarks filtered by a known tag and verifying only bookmarks carrying that tag are returned. Delivers value independently of read-status or text-search filters.

**Acceptance Scenarios**:

1. **Given** a collection of bookmarks with various tags, **When** the user filters by tag "react", **Then** only bookmarks tagged "react" are returned.
2. **Given** no bookmarks are tagged "obscure-tag", **When** the user filters by "obscure-tag", **Then** an empty list is returned (no error).
3. **Given** a bookmark tagged "React" (capital R), **When** the user filters by "react" (lowercase), **Then** that bookmark is included in results (tag filter is case-insensitive).

---

### User Story 2 — Filter by Read Status (Priority: P2)

A user wants to view only unread (or only read) bookmarks so they can focus on articles they haven't visited yet or revisit ones they have. Filtering by "all" restores the full list.

**Why this priority**: Read/unread is a binary status already tracked per bookmark; exposing it as a filter delivers immediate utility for managing a reading queue.

**Independent Test**: Can be fully tested by creating bookmarks with different read statuses, filtering by each status value, and verifying only the matching bookmarks appear.

**Acceptance Scenarios**:

1. **Given** a mix of read and unread bookmarks, **When** the user filters by "unread", **Then** only bookmarks with unread status are returned.
2. **Given** a mix of read and unread bookmarks, **When** the user filters by "read", **Then** only bookmarks with read status are returned.
3. **Given** the user has applied a read-status filter, **When** they clear the filter (select "all"), **Then** all bookmarks are returned.
4. **Given** all bookmarks are read, **When** the user filters by "unread", **Then** an empty list is returned (no error).

---

### User Story 3 — Search Bookmarks by Keyword (Priority: P3)

A user types a keyword and sees only bookmarks whose title or notes contain that keyword. The search is not case-sensitive and matches partial words. Clearing the search restores the full list.

**Why this priority**: Keyword search enables direct retrieval by content, complementing tag and status filters for larger collections.

**Independent Test**: Can be fully tested by creating bookmarks with known titles and notes, searching for a substring, and verifying only matching bookmarks appear.

**Acceptance Scenarios**:

1. **Given** a bookmark titled "React Hooks Guide", **When** the user searches for "hook", **Then** that bookmark is returned (partial, case-insensitive match in title).
2. **Given** a bookmark with notes "Good reference for TypeScript generics", **When** the user searches for "typescript", **Then** that bookmark is returned (partial, case-insensitive match in notes).
3. **Given** no bookmark title or notes contain "xyzzy", **When** the user searches for "xyzzy", **Then** an empty list is returned (no error).
4. **Given** an active search term, **When** the user clears the search, **Then** the full (possibly further filtered) list is restored.

---

### User Story 4 — Combine Multiple Filters (Priority: P4)

A user applies more than one filter simultaneously — for example, tag "react" AND unread AND keyword "hooks" — and sees only bookmarks that satisfy every active filter at once.

**Why this priority**: Filter combination multiplies the value of individual filters; it is lower priority than individual filters only because those must work first.

**Independent Test**: Can be fully tested by applying two or three filters simultaneously and verifying the result set is the intersection of each filter applied alone.

**Acceptance Scenarios**:

1. **Given** tag filter "tech" and read-status filter "unread" are both active, **When** the user retrieves bookmarks, **Then** only bookmarks tagged "tech" AND marked unread are returned.
2. **Given** tag filter "react", read-status filter "unread", and keyword "hook" are all active, **When** the user retrieves bookmarks, **Then** only bookmarks satisfying all three criteria are returned.
3. **Given** a valid combination of filters matches no bookmarks, **When** the user retrieves bookmarks, **Then** an empty list is returned (no error).

---

### Edge Cases

- A whitespace-only search term is treated as no search term (returns the full unfiltered list).
- A tag filter value that matches no existing tag returns an empty list, not an error.
- Filters are applied together using AND logic; OR logic across filter types is out of scope.
- Filtering on a non-existent read-status value (anything other than "read", "unread", or "all") is rejected with a clear validation message.
- Search matches against both title and notes independently; a bookmark appearing in either field is included.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow users to retrieve bookmarks filtered by a single tag (case-insensitive match against bookmark tags).
- **FR-002**: The system MUST allow users to retrieve bookmarks filtered by read status: "read", "unread", or "all" (no filter). Omitting the status filter is equivalent to "all".
- **FR-003**: The system MUST allow users to search bookmarks by a keyword that is matched case-insensitively as a substring against the bookmark title and the bookmark notes. The bookmark URL is explicitly excluded from keyword search.
- **FR-004**: All three filter dimensions (tag, read status, keyword) MUST be combinable in a single request; the result is the intersection of all active filters (AND logic).
- **FR-005**: A filter request that matches no bookmarks MUST return an empty list, never an error response.
- **FR-006**: When no filters are applied, the system MUST return all bookmarks (preserving existing behaviour).
- **FR-007**: A whitespace-only keyword MUST be treated as no keyword filter (does not narrow results).
- **FR-008**: An invalid read-status value MUST be rejected with a validation error.
- **FR-009**: An empty string tag value MUST be treated as no tag filter (equivalent to omitting the tag parameter).

### Key Entities

- **Bookmark**: The existing bookmark entity. No new fields are required; filtering operates on existing `Tags`, `IsRead`, `Title`, and `Notes` fields.
- **Filter Criteria**: A set of optional parameters — tag name, read status, and keyword — that collectively narrow the bookmark list. Not persisted; evaluated per request.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A filtered bookmark list is returned within 3 seconds for any valid combination of filters against a collection of up to 1,000 bookmarks.
- **SC-002**: Each of the three filter types (tag, read status, keyword) works correctly in isolation: 100% of bookmarks matching the filter are returned and 0% of non-matching bookmarks are included.
- **SC-003**: Any combination of the three filter types returns only bookmarks satisfying all active criteria simultaneously.
- **SC-004**: A filter request matching no bookmarks returns an empty list with a success status 100% of the time (never an error).
- **SC-005**: Keyword search is case-insensitive and matches partial strings in both title and notes fields.

## Clarifications

### Session 2026-06-16

- Q: Should keyword search also match against the bookmark URL? → A: No — keyword search is scoped to title and notes only; the URL field is explicitly excluded.
- Q: Should an empty string tag value be treated as "no tag filter" or a validation error? → A: Empty string = no tag filter (treated the same as omitting the tag parameter).
- Q: What is the expected upper bound on bookmark collection size for performance targets? → A: Up to 1,000 bookmarks.

## Assumptions

- Only a single tag can be specified per filter request; filtering by multiple tags simultaneously is out of scope for this feature.
- Text search uses simple substring matching; fuzzy matching, stemming, and relevance ranking are out of scope.
- Filtering is stateless — the user's active filters are not saved between sessions.
- Results are returned in the same order as the unfiltered list (creation date ascending).
- The filter feature operates on the existing full bookmark collection; pagination is out of scope (consistent with existing single-user, personal tool scope). The expected upper bound on collection size is 1,000 bookmarks.
- Bookmark `Tags` are stored as a list; tag filter matches if any element of that list equals the filter value (case-insensitive).
- `Notes` may be null on a bookmark; a null notes field is treated as an empty string for search purposes (never causes an error).
