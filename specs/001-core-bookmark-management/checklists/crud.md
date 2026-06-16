# Requirements Quality Checklist: Core Bookmark Management (CRUD)

**Purpose**: Unit-test the requirements for completeness, clarity, consistency, and coverage
across all CRUD operations before implementation planning.
**Created**: 2026-06-16
**Feature**: [spec.md](../spec.md)
**Audience**: Author self-review pre-`/speckit-plan`
**Depth**: Standard (~25 items)

---

## Requirement Completeness

- [ ] CHK001 Is the retrieve/read operation (get a single bookmark by ID, or list all bookmarks)
  included in requirements, or explicitly called out as out of scope for this feature? [Completeness, Gap]
- [ ] CHK002 Are the response payloads for successful create, update, and delete operations
  fully specified — including exactly which fields are returned in each response? [Completeness, Gap]
- [ ] CHK003 Are validation constraints for the `title` field fully specified — e.g., maximum
  length, minimum length, or allowed characters? [Completeness, Spec §FR-003]
- [ ] CHK004 Are all constraints on the `tags` field specified beyond deduplication — e.g.,
  maximum number of tags per bookmark, maximum tag length, allowed characters per tag? [Completeness, Gap]
- [ ] CHK005 Are requirements defined for how the system assigns the unique identifier — e.g.,
  is the format (UUID, integer, etc.) relevant to any requirement, or intentionally
  implementation-defined? [Completeness, Spec §FR-002]

## Requirement Clarity

- [ ] CHK006 Is "case-insensitive" URL uniqueness (FR-009) precisely scoped — does it apply to
  the full URL string or only specific components (e.g., scheme and host, but not path)? [Clarity, Spec §FR-009]
- [ ] CHK007 Is the partial update semantic (FR-005) unambiguous for the `tags` field — does
  omitting `tags` from an update leave them unchanged, and if so, how does a user
  explicitly clear all tags to an empty list? [Clarity, Spec §FR-005]
- [ ] CHK008 Is the meaning of "permanently removed" in FR-007 sufficient — does the spec need
  to state that no audit trail, recovery path, or soft-delete exists for deleted
  bookmarks? [Clarity, Spec §FR-007]
- [ ] CHK009 Is the error detail requirement for duplicate URLs (FR-004, FR-006) precise enough
  — does "includes the title" mean the full title string verbatim, or may it be
  truncated? [Clarity, Spec §FR-004, §FR-006]
- [ ] CHK010 Is the `http://` or `https://`-only rule in FR-010 complete — are other schemes
  (e.g., `ftp://`, `mailto:`, `file://`) explicitly excluded, or does the spec rely on
  the two listed schemes as an exhaustive allow-list? [Clarity, Spec §FR-010]

## Requirement Consistency

- [ ] CHK011 Are URL format validation rules (FR-010) applied consistently to both the create
  and update operations, with equal specificity in both FR-001 and FR-005? [Consistency, Spec §FR-010, §FR-005]
- [ ] CHK012 Do the duplicate URL error requirements (FR-004 and FR-006) specify identical error
  detail (conflicting bookmark's title) for both create and update flows, with no
  discrepancies? [Consistency, Spec §FR-004, §FR-006]
- [ ] CHK013 Is the `last-modified` timestamp update rule (Assumptions) consistent with
  partial update semantics in FR-005 — is it clear whether a request that changes
  no values still updates the timestamp? [Consistency, Spec §FR-005, §Assumptions]
- [ ] CHK014 Are the default-status rule (FR-001 — defaults to unread) and the partial-update
  rule (FR-005 — omitted fields unchanged) consistent and non-conflicting when
  status is omitted on an update vs. on a create? [Consistency, Spec §FR-001, §FR-005]

## Acceptance Criteria Quality

- [ ] CHK015 Are the timing targets in SC-001 ("under 3 seconds") and SC-002 ("within 2 seconds")
  defined under stated conditions — e.g., what data volume or network state is assumed? [Measurability, Spec §SC-001, §SC-002]
- [ ] CHK016 Is "immediately unavailable" in SC-004 quantified, or does it rely on an implicit
  definition that could be interpreted differently by different reviewers? [Clarity, Spec §SC-004]
- [ ] CHK017 Is SC-005's "100% of duplicate URL attempts rejected" bounded by the normalization
  rules in FR-009 — e.g., does it apply regardless of casing, whitespace, or trailing
  slashes in the submitted URL? [Measurability, Spec §SC-005, §FR-009]

## Scenario Coverage

- [ ] CHK018 Is the "update URL to the bookmark's own current URL" scenario addressed — should
  this be accepted (no-op) or rejected as a self-duplicate? [Coverage, Gap]
- [ ] CHK019 Are requirements defined for attempting to delete an already-deleted bookmark —
  should the response be idempotent (success) or a not-found error? [Coverage, Edge Case]
- [ ] CHK020 Is the "empty tags array on update" scenario explicitly covered — does sending
  `tags: []` clear all tags, or is it treated as "no change" under partial update
  semantics? [Coverage, Spec §FR-005, Gap]
- [ ] CHK021 Are simultaneous validation failures addressed — e.g., if a request contains both
  a missing title and a malformed URL, does the system return all errors at once or
  stop at the first? [Coverage, Gap]

## Edge Case Coverage

- [ ] CHK022 Are requirements defined for extremely long field values — e.g., URLs near browser
  maximum length limits, or notes with thousands of characters? [Edge Case, Gap]
- [ ] CHK023 Is the behavior specified for whitespace-only values — e.g., a title or notes field
  containing only spaces: is it treated as provided or as missing/empty? [Edge Case, Gap]
- [ ] CHK024 Is the behavior defined when a URL contains encoded characters or fragments
  (e.g., `https://example.com/page?q=test#section`) — are these treated as part of
  the unique URL for deduplication purposes? [Edge Case, Spec §FR-009]

## Non-Functional Requirements

- [ ] CHK025 Are data durability expectations stated — e.g., is it explicitly documented that
  bookmarks are not guaranteed to persist across system restarts for this version,
  as a known constraint of the current data store? [Completeness, Assumption]

## Notes

- Mark items `[x]` when satisfied; add inline findings for anything that needs spec update
- Unresolved items in Clarity or Consistency categories should be fixed in spec before `/speckit-plan`
- CHK001 and CHK002 (response payloads, retrieve operation) are the highest-priority gaps to resolve
- CHK007 and CHK020 (tag clear vs. no-change) are linked — resolving one resolves both
