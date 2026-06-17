# Tasks: Filtering and Search

**Input**: Design documents from `specs/002-filter-search/`

**Prerequisites**: plan.md ✅, spec.md ✅, data-model.md ✅, contracts/bookmarks.md ✅, research.md ✅, quickstart.md ✅

**Tests**: TDD — tests are written and confirmed failing before each corresponding implementation block.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm no new projects or packages are needed; existing toolchain is ready.

- [X] T001 Verify `backend/BookmarkManager.slnx` builds with `dotnet build backend/` (baseline check)
- [X] T002 Verify frontend tests pass with `npm test` from `frontend/` (baseline check)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure changes that MUST be complete before any user story implementation can begin.

**⚠️ CRITICAL**: All three tasks must complete before Phase 3 can start.

- [X] T003 Create `backend/BookmarkManager.Api/DTOs/BookmarkFilterRequest.cs` with nullable properties `Tag`, `Status`, and `Keyword` (all `string?`)
- [X] T004 Add `Task<IEnumerable<Bookmark>> GetFilteredAsync(string? tag, bool? isRead, string? keyword)` to `backend/BookmarkManager.Api/Repositories/IBookmarkRepository.cs`
- [X] T005 Update `GetAllAsync()` signature in `backend/BookmarkManager.Api/Services/IBookmarkService.cs` to `GetAllAsync(BookmarkFilterRequest filter)` (or add the filter overload alongside the existing signature per research.md Decision 5)

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 — Filter by Tag (Priority: P1) 🎯 MVP

**Goal**: Users can retrieve only bookmarks that carry a specific tag; tag comparison is case-insensitive; empty-string tag = no filter.

**Independent Test**: POST two bookmarks with different tags; GET with `?tag=<tag>` and verify only the matching bookmark is returned.

### Tests for User Story 1 ⚠️ Write FIRST — confirm FAILING before implementing

- [X] T006 [P] [US1] Append tag-filter service tests to `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`:
  - `GetAllAsync_WithTagFilter_ReturnsOnlyMatchingBookmarks`
  - `GetAllAsync_WithTagFilterCaseInsensitive_ReturnsMatch`
  - `GetAllAsync_WithEmptyTagFilter_ReturnsAll`
  - `GetAllAsync_WithTagFilter_NoMatches_ReturnsEmptyList`
- [X] T007 [P] [US1] Append tag-filter controller tests to `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`:
  - `GetAll_WithTagQueryParam_ReturnsFilteredResults` (HTTP GET `?tag=react`)
  - `GetAll_WithEmptyTagQueryParam_ReturnsAll` (HTTP GET `?tag=`)

### Implementation for User Story 1

- [X] T008 [US1] Add stub `GetFilteredAsync` to `backend/BookmarkManager.Api/Repositories/BookmarkRepository.cs` — returns all bookmarks with no filtering (makes tests compile and fail meaningfully)
- [X] T009 [US1] Implement tag-filter LINQ in `BookmarkRepository.cs` `GetFilteredAsync`: `WHERE tag IS NULL OR Tags.Any(t => t.ToLowerInvariant() == tag)` ordered by `CreatedAt ASC`
- [X] T010 [US1] Implement tag normalisation in `backend/BookmarkManager.Api/Services/BookmarkService.cs` `GetAllAsync(BookmarkFilterRequest)`: trim tag, treat empty/whitespace as null, lowercase before passing to repository
- [X] T011 [US1] Update `backend/BookmarkManager.Api/Controllers/BookmarksController.cs` `GetAll` action: replace no-arg call with `[FromQuery] BookmarkFilterRequest filter` parameter and delegate to `_service.GetAllAsync(filter)`
- [X] T012 [US1] Run `dotnet test backend/` — confirm all tag-filter tests pass

**Checkpoint**: US1 is fully functional. Tag filtering works end-to-end.

---

## Phase 4: User Story 2 — Filter by Read Status (Priority: P2)

**Goal**: Users can retrieve only read or only unread bookmarks; "all" or omitted status returns everything; invalid status returns 400.

**Independent Test**: POST bookmarks with `isRead: true` and `isRead: false`; GET with `?status=unread` and verify only the unread bookmark is returned.

### Tests for User Story 2 ⚠️ Write FIRST — confirm FAILING before implementing

- [X] T013 [P] [US2] Append read-status service tests to `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`:
  - `GetAllAsync_WithStatusUnread_ReturnsOnlyUnread`
  - `GetAllAsync_WithStatusRead_ReturnsOnlyRead`
  - `GetAllAsync_WithStatusAll_ReturnsAll`
  - `GetAllAsync_WithStatusOmitted_ReturnsAll`
  - `GetAllAsync_WithInvalidStatus_ThrowsArgumentException`
  - `GetAllAsync_WithStatusUnread_NoMatches_ReturnsEmptyList`
- [X] T014 [P] [US2] Append read-status controller tests to `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`:
  - `GetAll_WithStatusUnread_ReturnsFilteredResults` (HTTP GET `?status=unread`)
  - `GetAll_WithInvalidStatus_Returns400` (HTTP GET `?status=badvalue`)

### Implementation for User Story 2

- [X] T015 [US2] Extend `BookmarkRepository.cs` `GetFilteredAsync` to apply `isRead` predicate: `WHERE isRead IS NULL OR IsRead == isRead`
- [X] T016 [US2] Implement status normalisation in `BookmarkService.cs` `GetAllAsync`: map `"read"` → `true`, `"unread"` → `false`, `"all"`/null → `null`; throw `ArgumentException` for any other value (controller catches → 400)
- [X] T017 [US2] Run `dotnet test backend/` — confirm all read-status tests pass

**Checkpoint**: US1 and US2 both work independently. Tag and read-status filters are functional.

---

## Phase 5: User Story 3 — Search by Keyword (Priority: P3)

**Goal**: Users can search by keyword matched case-insensitively as a substring against title and notes; URL is excluded; whitespace-only keyword = no filter.

**Independent Test**: POST a bookmark with a known title; GET with `?q=<partial-word>` and verify the bookmark is returned.

### Tests for User Story 3 ⚠️ Write FIRST — confirm FAILING before implementing

- [X] T018 [P] [US3] Append keyword search service tests to `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`:
  - `GetAllAsync_WithKeyword_MatchesTitle`
  - `GetAllAsync_WithKeyword_MatchesNotes`
  - `GetAllAsync_WithKeyword_CaseInsensitiveMatch`
  - `GetAllAsync_WithKeyword_DoesNotMatchUrl`
  - `GetAllAsync_WithWhitespaceKeyword_ReturnsAll`
  - `GetAllAsync_WithKeyword_NoMatches_ReturnsEmptyList`
- [X] T019 [P] [US3] Append keyword search controller tests to `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`:
  - `GetAll_WithKeywordQueryParam_ReturnsFilteredResults` (HTTP GET `?q=hook`)
  - `GetAll_WithWhitespaceKeyword_ReturnsAll` (HTTP GET `?q=%20%20`)

### Implementation for User Story 3

- [X] T020 [US3] Extend `BookmarkRepository.cs` `GetFilteredAsync` to apply keyword predicate: `WHERE keyword IS NULL OR Title.ToLowerInvariant().Contains(keyword) OR (Notes != null AND Notes.ToLowerInvariant().Contains(keyword))`
- [X] T021 [US3] Implement keyword normalisation in `BookmarkService.cs` `GetAllAsync`: trim, lowercase; treat whitespace-only as null before passing to repository
- [X] T022 [US3] Run `dotnet test backend/` — confirm all keyword search tests pass

**Checkpoint**: US1, US2, and US3 all work independently. All three filter dimensions are functional.

---

## Phase 6: User Story 4 — Combine Multiple Filters (Priority: P4)

**Goal**: All three filters can be active simultaneously; result is their intersection (AND logic); empty intersection returns `[]`, not an error.

**Independent Test**: POST bookmarks covering different tag/status/keyword combinations; apply all three filters at once and verify only the bookmark satisfying every criterion is returned.

### Tests for User Story 4 ⚠️ Write FIRST — confirm FAILING before implementing

- [X] T023 [P] [US4] Append combined-filter service tests to `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`:
  - `GetAllAsync_WithTagAndStatus_ReturnsIntersection`
  - `GetAllAsync_WithAllThreeFilters_ReturnsIntersection`
  - `GetAllAsync_WithAllThreeFilters_NoMatches_ReturnsEmptyList`
- [X] T024 [P] [US4] Append combined-filter controller tests to `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`:
  - `GetAll_WithTagAndStatusAndKeyword_ReturnsIntersection` (HTTP GET `?tag=react&status=unread&q=hooks`)
  - `GetAll_WithCombinedFilters_NoMatches_Returns200EmptyArray`

### Implementation for User Story 4

- [X] T025 [US4] Verify `BookmarkRepository.cs` `GetFilteredAsync` applies all three predicates with AND logic — review the full LINQ chain to confirm chaining is correct; adjust if any predicate short-circuits incorrectly
- [X] T026 [US4] Run `dotnet test backend/` — confirm all combined-filter tests pass and no regressions in prior test cases

**Checkpoint**: All four backend user stories are complete and independently testable.

---

## Phase 7: Polish & Frontend

**Purpose**: Wire up the frontend filter UI; validate end-to-end via quickstart.md smoke tests.

### Frontend Service Layer

- [X] T027 [P] Append `getAll(filter)` tests to `frontend/src/services/bookmarkService.test.js`:
  - `getAll with tag filter builds correct query string`
  - `getAll with status filter builds correct query string`
  - `getAll with keyword filter appends q param`
  - `getAll with all three filters builds full query string`
  - `getAll with status=all omits status param`
  - `getAll with empty tag omits tag param`
  - `getAll with no args calls plain /api/bookmarks (backward compat)`
- [X] T028 Update `frontend/src/services/bookmarkService.js` `getAll` to accept an optional `filter` object and append non-empty/non-"all" values as query params (`?tag=`, `?status=`, `?q=`); backward-compatible (no-arg call still works)

### Frontend Filter Component

- [X] T029 [P] Create `frontend/src/components/BookmarkFilter/BookmarkFilter.test.jsx` with failing tests:
  - `renders tag input, status select, and keyword input`
  - `calls onFilterChange with updated tag after 300 ms debounce`
  - `calls onFilterChange immediately when status changes`
  - `calls onFilterChange with updated keyword after 300 ms debounce`
- [X] T030 Create `frontend/src/components/BookmarkFilter/BookmarkFilter.jsx`: text input for tag (debounced 300 ms), `<select>` for status (all / read / unread, immediate), text input for keyword (debounced 300 ms); calls `onFilterChange(filter)` on any change

### Frontend List Integration

- [X] T031 Append filter-aware tests to `frontend/src/components/BookmarkList/BookmarkList.test.jsx`:
  - `re-fetches with filter when filter prop changes`
  - `passes filter to bookmarkService.getAll`
- [X] T032 Update `frontend/src/components/BookmarkList/BookmarkList.jsx` to accept a `filter` prop; pass it to `bookmarkService.getAll(filter)` on every fetch; re-fetch whenever `filter` changes (add `filter` to the `useEffect` dependency array)

### App Wiring

- [X] T033 Update `frontend/src/App.jsx`:
  - Add `const [filter, setFilter] = useState({ tag: '', status: 'all', keyword: '' })`
  - Render `<BookmarkFilter onFilterChange={setFilter} />` above `<BookmarkList>`
  - Pass `filter` prop to `<BookmarkList filter={filter} />`

### Final Validation

- [X] T034 Run `npm test` from `frontend/` — confirm all new and existing tests pass
- [X] T035 Run quickstart.md smoke tests (curl commands in `specs/002-filter-search/quickstart.md`) against running backend — confirm all 7 scenarios produce expected results

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — verify immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user story phases
- **Phase 3 (US1 — Tag)**: Depends on Phase 2
- **Phase 4 (US2 — Status)**: Depends on Phase 2; can start alongside Phase 3 if on separate files
- **Phase 5 (US3 — Keyword)**: Depends on Phase 2; can start alongside Phases 3–4 if on separate files
- **Phase 6 (US4 — Combined)**: Depends on Phases 3–5 being complete (needs all three filter dims working)
- **Phase 7 (Polish/Frontend)**: Can start after Phase 2 for service layer; BookmarkList/App changes depend on BookmarkFilter

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependency on US2/US3
- **US2 (P2)**: Can start after Phase 2 — no dependency on US1/US3
- **US3 (P3)**: Can start after Phase 2 — no dependency on US1/US2
- **US4 (P4)**: Depends on US1 + US2 + US3 being complete

### Within Each User Story

- Tests MUST be written and confirmed failing before implementation
- Repository changes before service changes before controller changes
- Run `dotnet test` after each story to catch regressions

### Parallel Opportunities

- T006 and T007 can run in parallel (different test files)
- T013 and T014 can run in parallel
- T018 and T019 can run in parallel
- T023 and T024 can run in parallel
- T027 and T029 can run in parallel (different frontend files)
- T028 and T030 can start in parallel after their respective test tasks

---

## Parallel Example: User Story 1

```bash
# Launch test tasks together:
Task T006: "Append tag-filter service tests to BookmarkServiceTests.cs"
Task T007: "Append tag-filter controller tests to BookmarksControllerTests.cs"

# Then implement:
Task T008: "Add stub GetFilteredAsync to BookmarkRepository.cs"
Task T009: "Implement tag-filter LINQ in GetFilteredAsync"
Task T010: "Implement tag normalisation in BookmarkService.cs"
Task T011: "Update BookmarksController.cs GetAll to bind [FromQuery] filter"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup baseline verification
2. Complete Phase 2: Foundational (DTO + interface changes)
3. Complete Phase 3: User Story 1 (tag filter)
4. **STOP and VALIDATE**: Test tag filter end-to-end with curl
5. Proceed to US2 if validated

### Incremental Delivery

1. Setup + Foundational → infrastructure ready
2. US1 → tag filter working → demo/validate
3. US2 → read-status filter working → demo/validate
4. US3 → keyword search working → demo/validate
5. US4 → combined filters working → demo/validate
6. Phase 7 → frontend filter UI → full feature complete

---

## Notes

- [P] tasks = different files, no shared-state dependencies — can run in parallel
- [USn] label traces each task back to a specific user story for review and rollback
- `GetFilteredAsync` repository method accumulates all three filter predicates; each user story phase adds its predicate and tests — the method evolves incrementally from Phase 3 through Phase 5
- `GetAllAsync()` (no-arg) remains in `IBookmarkRepository` per research.md Decision 5 to avoid breaking existing unit tests
- Confirm tests FAIL before writing implementation code — this is the TDD gate
- Run `dotnet test` and `npm test` at each phase checkpoint to detect regressions early
