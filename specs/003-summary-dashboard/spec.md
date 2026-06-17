# Feature Specification: Summary Dashboard

**Feature Branch**: `003-summary-dashboard`

**Created**: 2026-06-16

**Status**: Draft

**Input**: User description: "Summary Dashboard (specs/003-summary-dashboard): Shows total bookmarks, total unread, and a breakdown by tag. Reflects current data without a page reload."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Bookmark Summary Counts (Priority: P1)

The user navigates to the dashboard to get an immediate overview of their bookmark collection — how many bookmarks exist in total and how many have not yet been read.

**Why this priority**: This is the core value of the dashboard. A user needs to know at a glance the size of their collection and how much reading remains.

**Independent Test**: Can be fully tested by opening the dashboard with a known set of bookmarks and verifying the displayed counts match the actual data.

**Acceptance Scenarios**:

1. **Given** the user has 10 bookmarks, 4 of which are unread, **When** the user views the dashboard, **Then** the dashboard shows "10" as the total bookmark count and "4" as the unread count.
2. **Given** the user has no bookmarks, **When** the user views the dashboard, **Then** the dashboard shows "0" for both total and unread counts.
3. **Given** all bookmarks are marked as read, **When** the user views the dashboard, **Then** the unread count shows "0".

---

### User Story 2 - View Tag Breakdown (Priority: P2)

The user wants to see how their bookmarks are distributed across tags, so they can understand which topics dominate their collection.

**Why this priority**: Tag-based organisation is central to the bookmark manager. Seeing counts per tag helps users navigate their reading list by topic.

**Independent Test**: Can be fully tested by opening the dashboard with bookmarks assigned to various tags and verifying each tag's count is correct.

**Acceptance Scenarios**:

1. **Given** the user has 5 bookmarks tagged "technology" and 3 tagged "design", **When** the user views the dashboard, **Then** the tag breakdown shows "technology: 5" and "design: 3".
2. **Given** a bookmark has multiple tags, **When** the user views the dashboard, **Then** that bookmark is counted once under each of its tags.
3. **Given** the user has bookmarks with no tags, **When** the user views the dashboard, **Then** untagged bookmarks appear as a separate "Untagged" count.
4. **Given** the user has no bookmarks, **When** the user views the dashboard, **Then** the tag breakdown section shows an appropriate empty state.

---

### User Story 3 - Live Data Synchronisation (Priority: P3)

The user adds, marks as read, or deletes a bookmark elsewhere in the application. The dashboard statistics update immediately without requiring a page reload.

**Why this priority**: A stale dashboard would mislead the user about the state of their collection during an active session.

**Independent Test**: Can be fully tested by modifying bookmarks and observing whether the dashboard counters update in the same session without a reload.

**Acceptance Scenarios**:

1. **Given** the dashboard is visible, **When** the user adds a new bookmark, **Then** the total count increases by 1 and the unread count increases by 1 (bookmarks default to unread).
2. **Given** the dashboard is visible, **When** the user marks a bookmark as read, **Then** the unread count decreases by 1.
3. **Given** the dashboard is visible, **When** the user deletes a bookmark, **Then** the total count decreases by 1 and the unread count decreases by 1 if the bookmark was unread.
4. **Given** the dashboard is visible, **When** the user adds a bookmark with a new tag, **Then** the tag breakdown updates to include the new tag.
5. **Given** the dashboard is visible, **When** the last bookmark for a tag is deleted, **Then** that tag disappears from the breakdown.

---

### Edge Cases

- What happens when all bookmarks are deleted? → Dashboard shows "0" for all counts and an empty state for the tag breakdown.
- What happens when the same tag appears on many bookmarks? → It is counted correctly regardless of volume.
- What happens if a bookmark has no tags? → It is counted in an "Untagged" category, separate from the tag breakdown rows.
- What happens if the data cannot be loaded? → Dashboard shows an appropriate error or loading message instead of stale or blank counts.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display the total number of bookmarks currently stored.
- **FR-002**: System MUST display the total number of bookmarks with an unread status.
- **FR-003**: System MUST display a breakdown showing the number of bookmarks assigned to each tag.
- **FR-004**: A bookmark with multiple tags MUST be counted once under each of its tags in the breakdown.
- **FR-005**: System MUST display an "Untagged" count for bookmarks that have no tags assigned, when that count is greater than zero.
- **FR-006**: Dashboard statistics MUST update automatically when bookmark data changes during the same user session, without requiring a manual page reload.
- **FR-007**: Dashboard MUST display an appropriate empty state when there are no bookmarks.
- **FR-008**: Dashboard MUST display an appropriate empty state for the tag breakdown when no bookmarks have tags.

### Key Entities

- **Dashboard Summary**: An aggregate read-only view of the bookmark collection. Contains total bookmark count, total unread count, and a list of tag statistics.
- **Tag Statistic**: A named category paired with the count of bookmarks assigned to it. Represents one row in the tag breakdown.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All summary counts (total, unread, per-tag) are accurate and match the actual data state at the time of viewing, with zero tolerance for incorrect counts.
- **SC-002**: Dashboard statistics reflect any bookmark change made in the same session within 1 second, without a page reload.
- **SC-003**: The dashboard correctly handles an empty collection, displaying zero counts and an appropriate empty state message.
- **SC-004**: The tag breakdown correctly represents all tags in the collection, with each tag's count matching the number of bookmarks assigned to it.
- **SC-005**: Users can interpret the dashboard at a glance — total, unread, and tag counts are each clearly labelled and visually distinct.

## Assumptions

- A bookmark can have zero or more tags; if it has multiple tags, it is counted under each of them in the tag breakdown.
- "Unread" status is an existing attribute of bookmarks established by the core bookmark management feature; this dashboard does not modify that attribute.
- The dashboard reflects the state of bookmarks within the current application session; cross-device or real-time server push synchronisation is out of scope.
- The dashboard is a read-only view — users cannot modify bookmarks directly from it.
- No authentication or user-specific filtering is required (single-user application per the project constitution).
- The dashboard is accessible from the main application navigation as a dedicated view or panel.
- Tags are case-sensitive strings; "Tech" and "tech" are treated as distinct tags (consistent with the core bookmark management feature).
