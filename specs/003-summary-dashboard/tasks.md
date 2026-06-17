# Tasks: Summary Dashboard

**Branch**: `003-summary-dashboard`
**Input**: Design documents from `specs/003-summary-dashboard/`

**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/summary.md ✅ quickstart.md ✅

**Tests**: Included — constitution Principle IV mandates test-first development (NON-NEGOTIABLE). Write each test task and confirm it fails before implementing the corresponding code.

**Organization**: Tasks grouped by user story. Backend infrastructure is foundational and blocks all three user stories.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Parallelizable — different files, no inter-task dependency at this point
- **[Story]**: Maps to user story (US1, US2, US3)
- Exact file paths included in every task

---

## Phase 1: Foundational — Backend Summary Infrastructure

**Purpose**: New DTOs, repository method, service method, and controller action that all three user stories depend on. No user story work begins until this phase is complete.

**⚠️ CRITICAL**: Passes constitution gates I–V. No layer skips. Tests written before implementation.

### Tests (write first — must fail before proceeding to implementation)

- [x] T001 Write failing xUnit tests for `BookmarkService.GetSummaryAsync()` covering: empty collection returns zeros, total count, unread count in `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`
- [x] T002 [P] Write failing xUnit tests for `GET /api/bookmarks/summary` covering: 200 response, correct JSON shape, empty collection response in `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`

### Implementation

- [x] T003 [P] Create `TagCount` record in `backend/BookmarkManager.Api/DTOs/TagCount.cs`
- [x] T004 [P] Create `BookmarkSummaryResponse` record in `backend/BookmarkManager.Api/DTOs/BookmarkSummaryResponse.cs`
- [x] T005 Add `Task<BookmarkSummaryResponse> GetSummaryAsync()` to `backend/BookmarkManager.Api/Repositories/IBookmarkRepository.cs` (depends on T003, T004)
- [x] T006 Implement `GetSummaryAsync()` in `backend/BookmarkManager.Api/Repositories/BookmarkRepository.cs` — LINQ aggregation: total, unread, per-tag counts (case-sensitive, alphabetical), untagged count (depends on T005)
- [x] T007 [P] Add `Task<BookmarkSummaryResponse> GetSummaryAsync()` to `backend/BookmarkManager.Api/Services/IBookmarkService.cs` (depends on T003, T004)
- [x] T008 Implement `GetSummaryAsync()` in `backend/BookmarkManager.Api/Services/BookmarkService.cs` — delegate to repository, return DTO (depends on T006, T007)
- [x] T009 Add `[HttpGet("summary")]` action to `backend/BookmarkManager.Api/Controllers/BookmarksController.cs` — returns `Ok(await service.GetSummaryAsync())` (depends on T008)

**Checkpoint**: ✅ `dotnet test` — 32 tests pass (4 new: 2 service + 2 controller).

---

## Phase 2: User Story 1 — View Bookmark Summary Counts (Priority: P1) 🎯 MVP

**Goal**: User sees total bookmarks and total unread displayed on the dashboard.

**Independent Test**: Open the app with a known set of bookmarks; verify the dashboard shows correct total and unread counts.

### Tests (write first — must fail before proceeding)

- [x] T010 [US1] Write failing Vitest tests for `BookmarkSummary` component: renders total count, renders unread count, renders empty state when collection is empty in `frontend/src/components/BookmarkSummary/BookmarkSummary.test.jsx`
- [x] T011 [P] [US1] Write failing Vitest test for `getSummary()` — mocks `fetch` and verifies it calls `/api/bookmarks/summary` in `frontend/src/services/bookmarkService.test.js`

### Implementation

- [x] T012 [P] [US1] Add `getSummary()` function to `frontend/src/services/bookmarkService.js`
- [x] T013 [US1] Create `BookmarkSummary` component: fetches summary via `getSummary()`, displays total and unread counts, shows empty state when total is 0, accepts `refresh` prop and re-fetches on change in `frontend/src/components/BookmarkSummary/BookmarkSummary.jsx` (depends on T012)
- [x] T014 [US1] Render `<BookmarkSummary refresh={refresh} />` in `frontend/src/App.jsx` (depends on T013)

**Checkpoint**: ✅ `npm test` — 28 tests pass.

---

## Phase 3: User Story 2 — View Tag Breakdown (Priority: P2)

**Goal**: User sees a per-tag bookmark count and a separate untagged count on the dashboard.

**Independent Test**: Open the app with bookmarks spread across multiple tags; verify the tag breakdown matches the expected per-tag counts and the untagged count is correct.

### Tests (write first — must fail before proceeding)

- [x] T015 [P] [US2] Write failing Vitest tests for `BookmarkSummary` tag breakdown: renders each tag with its count, renders untagged count when > 0, hides untagged row when untaggedCount is 0, renders empty tag breakdown message when no tags exist in `frontend/src/components/BookmarkSummary/BookmarkSummary.test.jsx`

### Implementation

- [x] T016 [US2] Update `BookmarkSummary` to render tag breakdown list and untagged count section in `frontend/src/components/BookmarkSummary/BookmarkSummary.jsx` (depends on T015)

**Checkpoint**: ✅ Tag breakdown and untagged count tested and implemented.

---

## Phase 4: User Story 3 — Live Data Synchronisation (Priority: P3)

**Goal**: Dashboard statistics update automatically after any bookmark mutation (create, update, delete) within the same session — no page reload required.

**Independent Test**: With the dashboard visible, add a bookmark, mark one as read, and delete one; verify the summary counters update after each action without reloading.

### Tests (write first — must fail before proceeding)

- [x] T017 [P] [US3] Write failing Vitest test for `BookmarkSummary`: component re-fetches when `refresh` prop increments in `frontend/src/components/BookmarkSummary/BookmarkSummary.test.jsx`
- [x] T018 [P] [US3] Write failing Vitest tests for `BookmarkList`: calls `onUpdated` prop after successful update, calls `onDeleted` prop after successful delete in `frontend/src/components/BookmarkList/BookmarkList.test.jsx`

### Implementation

- [x] T019 [US3] Update `BookmarkList` to accept optional `onUpdated` and `onDeleted` props; call them (if provided) after the existing local state mutation in `frontend/src/components/BookmarkList/BookmarkList.jsx` (depends on T018)
- [x] T020 [US3] Update `App.jsx`: add `handleUpdated` and `handleDeleted` functions (both call `setRefresh(r => r + 1)`); pass as `onUpdated`/`onDeleted` props to `BookmarkList` (depends on T019)

**Checkpoint**: ✅ `npm test` — all 28 tests pass including live sync tests.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end validation and final quality pass.

- [x] T021 Run quickstart.md smoke tests against running servers — all 7 curl assertions must produce expected values
- [ ] T022 [P] Verify UI renders correctly for edge cases: all bookmarks deleted (all zeros + empty tag state), single bookmark with multiple tags (each tag count correct)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: No dependencies — start immediately. Blocks all user stories.
- **US1 (Phase 2)**: Depends on Phase 1 complete (backend endpoint live)
- **US2 (Phase 3)**: Depends on Phase 2 complete (BookmarkSummary component exists)
- **US3 (Phase 4)**: Depends on Phase 1 complete; can start in parallel with Phase 3 (different files)
- **Polish (Phase 5)**: Depends on all story phases complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational (backend). No dependency on US2 or US3.
- **US2 (P2)**: Depends on US1 (extends the same component). Cannot be implemented independently of US1.
- **US3 (P3)**: Depends on Foundational and US1 (needs `BookmarkSummary` and `BookmarkList` to exist). Can overlap with US2.

### Within Each Phase

```
Tests MUST be written and verified FAILING → then implement → then verify PASSING
DTOs (T003, T004) are parallel → interfaces (T005, T007) → implementations (T006, T008) → controller (T009)
```

### Parallel Opportunities

- T001 and T002 can be written in parallel
- T003 and T004 can be written in parallel
- T005 and T007 can be written in parallel (different interfaces)
- T010 and T011 can be written in parallel
- T012 is parallel with T010/T011 writing
- T015 and T017/T018 can be written in parallel (different test files / different concerns)

---

## Parallel Example: Foundational Phase

```
# Write tests in parallel:
Task T001: xUnit service tests in BookmarkServiceTests.cs
Task T002: xUnit controller tests in BookmarksControllerTests.cs

# Create DTOs in parallel:
Task T003: TagCount.cs
Task T004: BookmarkSummaryResponse.cs

# Add interface methods in parallel:
Task T005: IBookmarkRepository.cs
Task T007: IBookmarkService.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Foundational backend (T001–T009)
2. Complete Phase 2: US1 — total + unread display (T010–T014)
3. **STOP and VALIDATE**: `dotnet test` + `npm test` + manual check at `http://localhost:3000`
4. Dashboard shows counts — MVP delivered

### Incremental Delivery

1. Foundational + US1 → Total/unread counts visible (MVP)
2. Add US2 → Tag breakdown visible
3. Add US3 → Live sync without reload
4. Polish → Smoke tests pass end-to-end

---

## Notes

- [P] tasks operate on different files with no mutual dependencies at that point — safe to run concurrently
- [US1/US2/US3] labels trace each task to the spec user story it satisfies
- Constitution Principle IV: every implementation task must have a failing test before code is written
- `BookmarkCard` already calls `onUpdated`/`onDeleted` → `BookmarkList` — no changes needed to `BookmarkCard.jsx`
- Route `GET /api/bookmarks/summary` does not conflict with `GET /api/bookmarks/{id:guid}` (GUID constraint prevents "summary" from matching)
