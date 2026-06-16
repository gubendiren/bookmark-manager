# Feature Specification: Core Bookmark Management

**Feature Branch**: `001-core-bookmark-management`

**Created**: 2026-06-16

**Status**: Draft

**Input**: User description: "Build the core bookmark management feature. A user can create a bookmark
with a URL, title, tags, notes, and a read or unread status. A user can update any of these fields.
A user can delete a bookmark. The system must prevent saving a duplicate URL and must return a clear
error when a duplicate is attempted."

## Clarifications

### Session 2026-06-16

- Q: Should the system validate that submitted URLs follow standard web URL format? → A: Yes — URL must include `http://` or `https://` scheme; any other format is a validation error.
- Q: When updating a bookmark, must the user provide all fields or only the fields being changed? → A: Partial update — only the fields being changed need to be provided; omitted fields remain unchanged.
- Q: What is the default read/unread status when a bookmark is created without specifying one? → A: Unread — bookmarks default to unread when status is not provided at creation.
- Q: If a bookmark submission contains the same tag more than once, what should the system do? → A: Silently deduplicate — repeated tags are removed automatically; no error is raised.
- Q: When a duplicate URL error is returned, should it reference the existing conflicting bookmark? → A: Yes — the error message MUST include the title of the existing bookmark.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Save a New Bookmark (Priority: P1)

A user adds a new bookmark by supplying a URL, title, optional tags, optional notes, and an initial
read/unread status. The system saves the bookmark and confirms it with a system-assigned identifier.

**Why this priority**: Creating a bookmark is the foundational action; the entire feature is worthless
without it. All other stories depend on bookmarks existing.

**Independent Test**: Can be fully tested by submitting a new bookmark through the interface and
confirming it appears with the correct fields and identifier. Delivers a minimal but useful saving
capability on its own.

**Acceptance Scenarios**:

1. **Given** no bookmark with the same URL exists, **When** the user submits a URL, title, tags, notes,
   and a read/unread status, **Then** the bookmark is saved and the system returns a confirmation
   with the new identifier and all submitted fields.
2. **Given** a bookmark with the same URL already exists, **When** the user attempts to save another
   bookmark with that URL, **Then** the system rejects the request and returns a clear, human-readable
   error message identifying it as a duplicate URL and including the title of the existing bookmark.
3. **Given** a bookmark form, **When** the user submits without a URL or without a title, **Then**
   the system rejects the request with a specific validation error naming the missing field.
4. **Given** the user submits a new bookmark without specifying a read/unread status, **When** the
   bookmark is saved, **Then** its status is set to unread by default.

---

### User Story 2 - Update an Existing Bookmark (Priority: P2)

A user selects an existing bookmark and modifies one or more of its fields: URL, title, tags, notes,
or read/unread status. Changes are persisted immediately.

**Why this priority**: Bookmarks need to be maintainable over time (e.g., marking as read, correcting
a title, adding tags). Depends on P1 bookmarks existing first.

**Independent Test**: Can be fully tested by creating a bookmark (P1 complete), then updating each
field individually and confirming the changes are reflected on retrieval.

**Acceptance Scenarios**:

1. **Given** an existing bookmark, **When** the user changes any field (e.g., marks it as read,
   updates the title, adds a tag), **Then** the changes are saved and returned in the updated bookmark.
2. **Given** an existing bookmark, **When** the user updates its URL to one already used by a
   different bookmark, **Then** the system rejects the update with a clear duplicate URL error
   that includes the title of the conflicting bookmark.
3. **Given** a bookmark identifier that does not exist, **When** the user attempts an update,
   **Then** the system returns a clear not-found error.

---

### User Story 3 - Delete a Bookmark (Priority: P3)

A user permanently removes a bookmark from the system by its identifier. The bookmark is immediately
unavailable after deletion.

**Why this priority**: Deletion keeps the list manageable. Depends on bookmarks existing (P1) but
is independently useful without P2.

**Independent Test**: Can be fully tested by creating a bookmark (P1 complete), deleting it, and
confirming it is no longer retrievable.

**Acceptance Scenarios**:

1. **Given** an existing bookmark, **When** the user deletes it by identifier, **Then** the bookmark
   is permanently removed and confirmed as deleted.
2. **Given** a bookmark identifier that does not exist, **When** the user attempts to delete it,
   **Then** the system returns a clear not-found error.

---

### Edge Cases

- What happens when a URL is submitted with leading/trailing whitespace?
- What happens when a URL is submitted with different letter casing (e.g., `HTTP://` vs `http://`)?
- What happens when a URL is submitted without a scheme (e.g., `example.com`)? → Rejected with a URL format validation error.
- What happens when tags are an empty list vs not provided?
- What happens when notes are not provided?
- What happens when a user attempts to update a bookmark with no changed fields?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow a user to create a bookmark with a URL, title, zero or more tags,
  optional notes, and an optional read/unread status (defaults to unread if not provided).
- **FR-002**: System MUST assign a unique identifier to each bookmark upon creation.
- **FR-003**: System MUST reject bookmark creation when URL or title is not provided, returning
  a validation error that identifies the missing field(s).
- **FR-010**: System MUST reject bookmark creation or update when the submitted URL does not include
  a valid `http://` or `https://` scheme, returning a validation error identifying the format issue.
- **FR-004**: System MUST reject bookmark creation when the submitted URL duplicates an existing
  bookmark's URL, returning a human-readable error message that identifies the conflict and includes
  the title of the existing bookmark.
- **FR-005**: System MUST allow a user to update one or more fields of an existing bookmark (URL,
  title, tags, notes, read/unread status) in a single operation; fields not included in the request
  MUST remain unchanged.
- **FR-006**: System MUST reject a bookmark update if the new URL duplicates the URL of a different
  existing bookmark, returning a human-readable error message that includes the title of the
  conflicting bookmark.
- **FR-007**: System MUST allow a user to permanently delete a bookmark by its identifier.
- **FR-008**: System MUST return a clear not-found error when an update or delete targets a bookmark
  identifier that does not exist.
- **FR-009**: URL uniqueness checks MUST be case-insensitive and ignore leading/trailing whitespace.

### Key Entities

- **Bookmark**: Represents a saved web resource. Attributes: unique identifier, URL, title,
  tags (list of text labels), notes (free-form text), read/unread status, creation timestamp,
  last-modified timestamp.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can save a new bookmark and receive confirmation in under 3 seconds.
- **SC-002**: A duplicate URL attempt surfaces a descriptive error message within 2 seconds.
- **SC-003**: Any subset of bookmark fields (URL, title, tags, notes, status) can be updated in a
  single operation; fields not included in the request retain their existing values without data loss.
- **SC-004**: A deleted bookmark is immediately unavailable upon receiving deletion confirmation.
- **SC-005**: 100% of attempts to save or update a bookmark with a duplicate URL are rejected
  with a message that names the conflicting URL and the title of the existing bookmark, enabling
  the user to resolve the conflict without further assistance.

## Assumptions

- The application serves a single user; no per-bookmark ownership or access control is required.
- Tags are stored as a flat list of text strings; no hierarchy or taxonomy is defined. Duplicate
  tags within a single bookmark are silently removed by the system.
- Notes are free-form text with no length limit or formatting requirements for this version.
- Read/unread is a binary status; no partial or in-progress state exists. When not specified at
  creation, status defaults to unread.
- Bookmarks are permanently deleted — no soft-delete or archive mechanism is in scope.
- URL uniqueness is enforced across all saved bookmarks; two bookmarks cannot share the same
  URL after normalisation (trimmed, case-insensitive).
- Last-modified timestamp is updated automatically by the system on every successful update.
