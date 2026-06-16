# Requirements Quality Checklist: Core Bookmark Management (CRUD)

**Purpose**: Unit-test the requirements for completeness, clarity, consistency, and coverage
across all CRUD operations before implementation planning.
**Created**: 2026-06-16
**Feature**: [spec.md](../spec.md)
**Audience**: Author self-review pre-`/speckit-plan`
**Depth**: Standard (~25 items)

---

## Requirement Completeness

- [x] CHK001 Is the retrieve/read operation (get a single bookmark by ID, or list all bookmarks)
  included in requirements, or explicitly called out as out of scope for this feature? [Completeness, Gap]
  → Resolved in `/speckit-plan` research.md Decision 6: GET /api/bookmarks and GET /api/bookmarks/{id}
  added; fully documented in contracts/bookmarks.md.
- [x] CHK002 Are the response payloads for successful create, update, and delete operations
  fully specified — including exactly which fields are returned in each response? [Completeness, Gap]
  → Resolved: contracts/bookmarks.md specifies full BookmarkResponse (id, url, title, tags, notes,
  isRead, createdAt, lastModifiedAt) for 201/200; 204 No Content for DELETE.
- [x] CHK003 Are validation constraints for the `title` field fully specified — e.g., maximum
  length, minimum length, or allowed characters? [Completeness, Spec §FR-003]
  → Resolved in data-model.md: Title max 500 chars; whitespace-only treated as missing.
- [x] CHK004 Are all constraints on the `tags` field specified beyond deduplication — e.g.,
  maximum number of tags per bookmark, maximum tag length, allowed characters per tag? [Completeness, Gap]
  → Resolved in data-model.md: each tag max 100 chars. No max tag count — intentional (YAGNI,
  Constitution Principle V; single-user personal tool with no scalability constraints).
- [x] CHK005 Are requirements defined for how the system assigns the unique identifier — e.g.,
  is the format (UUID, integer, etc.) relevant to any requirement, or intentionally
  implementation-defined? [Completeness, Spec §FR-002]
  → Resolved in data-model.md: Id is a Guid (UUID v4), system-generated at creation.

## Requirement Clarity

- [x] CHK006 Is "case-insensitive" URL uniqueness (FR-009) precisely scoped — does it apply to
  the full URL string or only specific components (e.g., scheme and host, but not path)? [Clarity, Spec §FR-009]
  → Resolved in research.md Decision 2 and data-model.md: full URL lowercased via
  `url.Trim().ToLowerInvariant()`.
- [x] CHK007 Is the partial update semantic (FR-005) unambiguous for the `tags` field — does
  omitting `tags` from an update leave them unchanged, and if so, how does a user
  explicitly clear all tags to an empty list? [Clarity, Spec §FR-005]
  → Resolved in research.md Decision 1, data-model.md, and contracts/bookmarks.md:
  omit/null = unchanged; `[]` = clear all tags.
- [x] CHK008 Is the meaning of "permanently removed" in FR-007 sufficient — does the spec need
  to state that no audit trail, recovery path, or soft-delete exists for deleted
  bookmarks? [Clarity, Spec §FR-007]
  → Resolved in spec Assumptions: "Bookmarks are permanently deleted — no soft-delete or archive
  mechanism is in scope."
- [x] CHK009 Is the error detail requirement for duplicate URLs (FR-004, FR-006) precise enough
  — does "includes the title" mean the full title string verbatim, or may it be
  truncated? [Clarity, Spec §FR-004, §FR-006]
  → Resolved in contracts/bookmarks.md: error `detail` field shows the full title verbatim
  (e.g., `"This URL is already saved as \"React Docs\"."`) and `conflictingBookmark.title` field.
- [x] CHK010 Is the `http://` or `https://`-only rule in FR-010 complete — are other schemes
  (e.g., `ftp://`, `mailto:`, `file://`) explicitly excluded, or does the spec rely on
  the two listed schemes as an exhaustive allow-list? [Clarity, Spec §FR-010]
  → Resolved in research.md Decision 2: regex `^https?://` is an exhaustive allow-list;
  all other schemes are implicitly rejected.

## Requirement Consistency

- [x] CHK011 Are URL format validation rules (FR-010) applied consistently to both the create
  and update operations, with equal specificity in both FR-001 and FR-005? [Consistency, Spec §FR-010, §FR-005]
  → Resolved: FR-010 explicitly covers both creation and update; confirmed in contracts/bookmarks.md
  (PATCH 400 response includes URL format error).
- [x] CHK012 Do the duplicate URL error requirements (FR-004 and FR-006) specify identical error
  detail (conflicting bookmark's title) for both create and update flows, with no
  discrepancies? [Consistency, Spec §FR-004, §FR-006]
  → Resolved: both FR-004 and FR-006 require the conflicting bookmark's title; contracts show
  identical 409 response shape for POST and PATCH.
- [x] CHK013 Is the `last-modified` timestamp update rule (Assumptions) consistent with
  partial update semantics in FR-005 — is it clear whether a request that changes
  no values still updates the timestamp? [Consistency, Spec §FR-005, §Assumptions]
  → Resolved in data-model.md ("updated on every successful PATCH") and tasks.md T029
  ("always update LastModifiedAt to UTC now"). A no-op PATCH does update the timestamp.
- [x] CHK014 Are the default-status rule (FR-001 — defaults to unread) and the partial-update
  rule (FR-005 — omitted fields unchanged) consistent and non-conflicting when
  status is omitted on an update vs. on a create? [Consistency, Spec §FR-001, §FR-005]
  → Resolved: non-conflicting by design. Create: omit isRead → defaults to false (unread).
  Update: omit isRead → current value preserved. Different operations, different semantics,
  both explicitly documented.

## Acceptance Criteria Quality

- [x] CHK015 Are the timing targets in SC-001 ("under 3 seconds") and SC-002 ("within 2 seconds")
  defined under stated conditions — e.g., what data volume or network state is assumed? [Measurability, Spec §SC-001, §SC-002]
  → Acceptable for this scope: plan.md Technical Context defines the operating conditions as
  local development with in-memory storage and single-user scale. Implicit condition is sufficient
  for a personal tool; no external network or large dataset applies.
- [x] CHK016 Is "immediately unavailable" in SC-004 quantified, or does it rely on an implicit
  definition that could be interpreted differently by different reviewers? [Clarity, Spec §SC-004]
  → Resolved by architecture: synchronous in-memory API means "immediately" = before the
  204 response is returned. No async processing, no eventual consistency.
- [x] CHK017 Is SC-005's "100% of duplicate URL attempts rejected" bounded by the normalization
  rules in FR-009 — e.g., does it apply regardless of casing, whitespace, or trailing
  slashes in the submitted URL? [Measurability, Spec §SC-005, §FR-009]
  → Resolved: SC-005 is bounded by FR-009 (case-insensitive, whitespace-trimmed) and
  research.md Decision 2 (fragments and query strings are part of the key). "Same URL" is
  precisely defined.

## Scenario Coverage

- [x] CHK018 Is the "update URL to the bookmark's own current URL" scenario addressed — should
  this be accepted (no-op) or rejected as a self-duplicate? [Coverage, Gap]
  → Resolved in research.md Decision 2 and data-model.md: "A PATCH that sets url to the same
  normalized value as the bookmark's current url is accepted (not treated as a self-duplicate)."
- [x] CHK019 Are requirements defined for attempting to delete an already-deleted bookmark —
  should the response be idempotent (success) or a not-found error? [Coverage, Edge Case]
  → Resolved in contracts/bookmarks.md DELETE endpoint: "404 Not Found — no bookmark with
  this ID (including already-deleted bookmarks)." Non-idempotent by design.
- [x] CHK020 Is the "empty tags array on update" scenario explicitly covered — does sending
  `tags: []` clear all tags, or is it treated as "no change" under partial update
  semantics? [Coverage, Spec §FR-005, Gap]
  → Resolved in research.md Decision 1, data-model.md, and contracts/bookmarks.md:
  `"tags": []` clears all tags. Distinct from omitting the field (no change).
- [x] CHK021 Are simultaneous validation failures addressed — e.g., if a request contains both
  a missing title and a malformed URL, does the system return all errors at once or
  stop at the first? [Coverage, Gap]
  → Resolved in research.md Decision 4: ASP.NET Core ValidationProblemDetails returns all
  failing fields at once. Confirmed in contracts/bookmarks.md 400 response example.

## Edge Case Coverage

- [x] CHK022 Are requirements defined for extremely long field values — e.g., URLs near browser
  maximum length limits, or notes with thousands of characters? [Edge Case, Gap]
  → Resolved in data-model.md: URL max 2048 chars, Title max 500 chars, Notes max 10 000 chars,
  each tag max 100 chars.
- [x] CHK023 Is the behavior specified for whitespace-only values — e.g., a title or notes field
  containing only spaces: is it treated as provided or as missing/empty? [Edge Case, Gap]
  → Resolved in research.md Decision 5: whitespace-only URL and Title = validation error;
  whitespace-only Notes = stored as null.
- [x] CHK024 Is the behavior defined when a URL contains encoded characters or fragments
  (e.g., `https://example.com/page?q=test#section`) — are these treated as part of
  the unique URL for deduplication purposes? [Edge Case, Spec §FR-009]
  → Resolved in research.md Decision 2 and data-model.md: fragments and query strings are
  included in the normalization key — they are part of URL identity.

## Non-Functional Requirements

- [x] CHK025 Are data durability expectations stated — e.g., is it explicitly documented that
  bookmarks are not guaranteed to persist across system restarts for this version,
  as a known constraint of the current data store? [Completeness, Assumption]
  → Resolved: plan.md Technical Context states "data does not persist across application
  restarts; this is a known and accepted constraint"; quickstart.md repeats this explicitly.

## Notes

- All 25 items resolved across `/speckit-clarify`, `/speckit-plan`, and associated design artifacts.
- CHK001/CHK002: GET endpoints and response payloads added during planning (research.md Decision 6/7).
- CHK007/CHK020: Tag partial-update semantics (null vs []) defined in research.md Decision 1.
- CHK013: No-op PATCH updates LastModifiedAt — defined in data-model.md and tasks.md T029.
- CHK018/CHK019: Self-URL and idempotent-delete decisions made in research.md and contracts.
- Checklist re-evaluated 2026-06-16 post-planning: 25/25 items pass.
