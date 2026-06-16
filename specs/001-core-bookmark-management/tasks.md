---

description: "Task list for Core Bookmark Management"
---

# Tasks: Core Bookmark Management

**Input**: Design documents from `specs/001-core-bookmark-management/`

**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/bookmarks.md ✅

**Tests**: Included — required by Constitution Principle IV (Test-First, NON-NEGOTIABLE).
Write each test task and confirm it FAILS before implementing the corresponding code.

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Exact file paths included in every task description

## Path Conventions

- Backend: `backend/BookmarkManager.Api/` and `backend/BookmarkManager.Api.Tests/`
- Frontend: `frontend/src/`

---

## Phase 1: Setup

**Purpose**: Initialize both projects and install all dependencies

- [X] T001 Create .NET 8 Web API solution with two projects (`BookmarkManager.Api` and `BookmarkManager.Api.Tests`) at `backend/`
- [X] T002 Create React 18 application at `frontend/` using Create React App or Vite
- [X] T003 [P] Add EF Core In-Memory (`Microsoft.EntityFrameworkCore.InMemory`) and FluentAssertions NuGet packages to `backend/BookmarkManager.Api/BookmarkManager.Api.csproj`; add xUnit and project reference to `backend/BookmarkManager.Api.Tests/BookmarkManager.Api.Tests.csproj`
- [X] T004 [P] Add `@testing-library/react`, `@testing-library/jest-dom`, and `@testing-library/user-event` to `frontend/package.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story begins

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 Create `Bookmark` entity class with properties `Id` (Guid), `Url`, `Title`, `Tags` (List\<string\>), `Notes` (string?), `IsRead` (bool), `CreatedAt` (DateTime), `LastModifiedAt` (DateTime) in `backend/BookmarkManager.Api/Models/Bookmark.cs`
- [X] T006 [P] Create `BookmarkDbContext` inheriting `DbContext` with `DbSet<Bookmark> Bookmarks` property in `backend/BookmarkManager.Api/Data/BookmarkDbContext.cs`
- [X] T007 [P] Create `CreateBookmarkRequest` (url, title, tags?, notes?, isRead?), `UpdateBookmarkRequest` (all fields nullable — null means no change, `tags: []` clears all tags), and `BookmarkResponse` (all fields including createdAt, lastModifiedAt) DTOs in `backend/BookmarkManager.Api/DTOs/`
- [X] T008 Define `IBookmarkRepository` interface with methods `CreateAsync`, `GetAllAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsByNormalizedUrlAsync`, `GetByNormalizedUrlAsync` in `backend/BookmarkManager.Api/Repositories/IBookmarkRepository.cs`
- [X] T009 [P] Define `IBookmarkService` interface with methods `CreateAsync`, `GetAllAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync` in `backend/BookmarkManager.Api/Services/IBookmarkService.cs`
- [X] T010 Register `BookmarkDbContext` (UseInMemoryDatabase "BookmarkDb"), `IBookmarkRepository → BookmarkRepository` (Scoped), `IBookmarkService → BookmarkService` (Scoped), CORS policy allowing `http://localhost:3000`, and ProblemDetails middleware in `backend/BookmarkManager.Api/Program.cs`
- [X] T011 [P] Configure API proxy so requests to `/api` are forwarded to `http://localhost:5000` in `frontend/package.json` (CRA) or `frontend/vite.config.js` (Vite)

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Save a New Bookmark (Priority: P1) 🎯 MVP

**Goal**: A user can create a bookmark and see it in the full list. Duplicate URLs are rejected with the conflicting bookmark's title in the error message.

**Independent Test**: POST a new bookmark → 201 with all fields. GET /api/bookmarks → list contains it. POST same URL again → 409 naming the first bookmark's title. Frontend form submits, list refreshes.

### Tests for User Story 1 ⚠️ Write FIRST — confirm they FAIL before implementing

- [X] T012 [P] [US1] Write failing xUnit tests for `BookmarkService.CreateAsync` — happy path returns BookmarkResponse, duplicate URL throws with conflicting title, missing URL returns validation error, missing title returns validation error, invalid URL scheme (no http/https) returns validation error — in `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`
- [X] T013 [P] [US1] Write failing xUnit tests for `POST /api/bookmarks` (201 + Location header), `GET /api/bookmarks` (200 array), and `GET /api/bookmarks/{id}` (200 or 404) endpoints in `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`
- [X] T014 [P] [US1] Write failing Jest tests for `bookmarkService.createBookmark(data)` and `bookmarkService.getAll()` — mock fetch, assert correct URLs and methods — in `frontend/src/services/bookmarkService.test.js`
- [X] T015 [P] [US1] Write failing Jest tests for `BookmarkForm` (renders fields, submits, displays 409 error) and `BookmarkList` (renders BookmarkCards on load) and `BookmarkCard` (displays all bookmark fields) in `frontend/src/components/BookmarkForm/BookmarkForm.test.jsx`, `BookmarkList/BookmarkList.test.jsx`, `BookmarkCard/BookmarkCard.test.jsx`

### Implementation for User Story 1

- [X] T016 [US1] Implement `BookmarkRepository` — `CreateAsync` (assigns Guid, sets timestamps), `GetAllAsync` (ordered by CreatedAt asc), `GetByIdAsync`, `ExistsByNormalizedUrlAsync` (normalize: `url.Trim().ToLowerInvariant()`), `GetByNormalizedUrlAsync` — in `backend/BookmarkManager.Api/Repositories/BookmarkRepository.cs`
- [X] T017 [US1] Implement `BookmarkService.CreateAsync` — validate URL matches `^https?://`, check duplicate via normalized URL (return 409 with conflicting bookmark title), deduplicate tags (case-insensitive, preserve order), default `IsRead = false` when not provided, set `CreatedAt` and `LastModifiedAt` to UTC now — in `backend/BookmarkManager.Api/Services/BookmarkService.cs`
- [X] T018 [US1] Implement `BookmarkService.GetAllAsync` and `BookmarkService.GetByIdAsync` (throw NotFoundException when not found) in `backend/BookmarkManager.Api/Services/BookmarkService.cs`
- [X] T019 [US1] Implement `POST /api/bookmarks` (201 Created + Location header), `GET /api/bookmarks` (200 array), and `GET /api/bookmarks/{id}` (200 or 404 ProblemDetails) in `backend/BookmarkManager.Api/Controllers/BookmarksController.cs`
- [X] T020 [P] [US1] Implement `bookmarkService.createBookmark(data)` (POST, return created bookmark) and `bookmarkService.getAll()` (GET, return array) in `frontend/src/services/bookmarkService.js`
- [X] T021 [P] [US1] Implement `BookmarkCard` component — display url (as clickable link), title, tags (as chips), notes, isRead badge, createdAt — in `frontend/src/components/BookmarkCard/BookmarkCard.jsx`
- [X] T022 [US1] Implement `BookmarkForm` component — inputs for url, title, tags (comma-separated), notes, isRead checkbox; on submit calls `createBookmark`; on 409 displays "Already saved as '[title]'" error; clears form on success — in `frontend/src/components/BookmarkForm/BookmarkForm.jsx`
- [X] T023 [US1] Implement `BookmarkList` component — calls `getAll()` on mount, renders a `BookmarkCard` for each result, shows "No bookmarks yet" when empty — in `frontend/src/components/BookmarkList/BookmarkList.jsx`
- [X] T024 [US1] Wire `BookmarkForm` and `BookmarkList` in `frontend/src/App.jsx`; pass a `onCreated` callback so `BookmarkList` refreshes after a successful create

**Checkpoint**: User Story 1 fully functional. Verify manually with quickstart.md smoke tests (POST + GET). All backend and frontend tests must pass before advancing.

---

## Phase 4: User Story 2 — Update an Existing Bookmark (Priority: P2)

**Goal**: A user can modify any subset of a bookmark's fields via a partial update. Fields not included in the request remain unchanged. Clearing all tags is supported by sending an empty array.

**Independent Test**: Create a bookmark (US1 complete). PATCH with `{"isRead": true}` → only `isRead` changes; all other fields unchanged. PATCH `{"tags": []}` → tags cleared. PATCH with a duplicate URL → 409 with conflicting title. PATCH a non-existent ID → 404.

### Tests for User Story 2 ⚠️ Write FIRST — confirm they FAIL before implementing

- [X] T025 [P] [US2] Write failing xUnit tests for `BookmarkService.UpdateAsync` — partial update preserves untouched fields, `tags: null` leaves tags unchanged, `tags: []` clears all tags, updating URL to own current URL is accepted, updating URL to another bookmark's URL returns 409 with conflicting title, not-found throws NotFoundException — appended to `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`
- [X] T026 [P] [US2] Write failing xUnit tests for `PATCH /api/bookmarks/{id}` (200 updated bookmark, 400, 404, 409) appended to `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`
- [X] T027 [P] [US2] Write failing Jest tests for `bookmarkService.updateBookmark(id, fields)` (PATCH, partial body) appended to `frontend/src/services/bookmarkService.test.js`
- [X] T028 [P] [US2] Write failing Jest tests for `BookmarkCard` edit mode — edit button toggles inline fields, save calls `updateBookmark`, cancel restores original values, 409 error displays conflicting title — in `frontend/src/components/BookmarkCard/BookmarkCard.test.jsx`

### Implementation for User Story 2

- [X] T029 [US2] Implement `BookmarkRepository.UpdateAsync` — apply only non-null fields from `UpdateBookmarkRequest`; for tags: null means skip, empty array means clear; always update `LastModifiedAt` to UTC now — in `backend/BookmarkManager.Api/Repositories/BookmarkRepository.cs`
- [X] T030 [US2] Implement `BookmarkService.UpdateAsync` — load bookmark (throw NotFoundException if missing); for each non-null field: validate URL format if url provided, check duplicate excluding self (same normalized URL as this bookmark's own URL is accepted), deduplicate tags if provided — in `backend/BookmarkManager.Api/Services/BookmarkService.cs`
- [X] T031 [US2] Implement `PATCH /api/bookmarks/{id}` (200 with full `BookmarkResponse`, 400 ProblemDetails, 404 ProblemDetails, 409 ProblemDetails with `conflictingBookmark`) in `backend/BookmarkManager.Api/Controllers/BookmarksController.cs`
- [X] T032 [P] [US2] Implement `bookmarkService.updateBookmark(id, fields)` (PATCH with partial body, return updated bookmark) in `frontend/src/services/bookmarkService.js`
- [X] T033 [US2] Add inline edit mode to `BookmarkCard` component — "Edit" button switches to editable inputs for all fields; "Save" calls `updateBookmark` and exits edit mode; "Cancel" discards changes; on 409 shows "URL already saved as '[title]'" — in `frontend/src/components/BookmarkCard/BookmarkCard.jsx`
- [X] T034 [US2] Update `BookmarkList` to replace the edited card in local state after a successful update without re-fetching the full list — in `frontend/src/components/BookmarkList/BookmarkList.jsx`

**Checkpoint**: User Stories 1 and 2 independently functional. Edit a bookmark, verify field preservation. Run full test suite.

---

## Phase 5: User Story 3 — Delete a Bookmark (Priority: P3)

**Goal**: A user can permanently remove a bookmark. It is immediately unavailable after deletion. Attempting to delete a non-existent bookmark returns 404.

**Independent Test**: Create a bookmark (US1 complete). DELETE it → 204. GET /api/bookmarks/{id} → 404. DELETE same ID again → 404. Frontend card disappears immediately.

### Tests for User Story 3 ⚠️ Write FIRST — confirm they FAIL before implementing

- [X] T035 [P] [US3] Write failing xUnit tests for `BookmarkService.DeleteAsync` — happy path succeeds, deleting non-existent ID throws NotFoundException — appended to `backend/BookmarkManager.Api.Tests/Services/BookmarkServiceTests.cs`
- [X] T036 [P] [US3] Write failing xUnit tests for `DELETE /api/bookmarks/{id}` (204, 404) appended to `backend/BookmarkManager.Api.Tests/Controllers/BookmarksControllerTests.cs`
- [X] T037 [P] [US3] Write failing Jest tests for `bookmarkService.deleteBookmark(id)` (DELETE, expect 204) appended to `frontend/src/services/bookmarkService.test.js`
- [X] T038 [P] [US3] Write failing Jest tests for delete button in `BookmarkCard` — button visible, calls `deleteBookmark` on confirm, calls `onDeleted` callback on success — appended to `frontend/src/components/BookmarkCard/BookmarkCard.test.jsx`

### Implementation for User Story 3

- [X] T039 [US3] Implement `BookmarkRepository.DeleteAsync` — remove by Id, return false if not found — in `backend/BookmarkManager.Api/Repositories/BookmarkRepository.cs`
- [X] T040 [US3] Implement `BookmarkService.DeleteAsync` — call `DeleteAsync` on repository, throw NotFoundException if it returns false — in `backend/BookmarkManager.Api/Services/BookmarkService.cs`
- [X] T041 [US3] Implement `DELETE /api/bookmarks/{id}` (204 No Content or 404 ProblemDetails) in `backend/BookmarkManager.Api/Controllers/BookmarksController.cs`
- [X] T042 [P] [US3] Implement `bookmarkService.deleteBookmark(id)` (DELETE, no response body expected) in `frontend/src/services/bookmarkService.js`
- [X] T043 [US3] Add "Delete" button to `BookmarkCard` component — shows inline confirmation ("Are you sure?"); on confirm calls `deleteBookmark`; on success calls `onDeleted(id)` prop — in `frontend/src/components/BookmarkCard/BookmarkCard.jsx`
- [X] T044 [US3] Update `BookmarkList` to remove the deleted bookmark from local state by id when `onDeleted` is called, without re-fetching the list — in `frontend/src/components/BookmarkList/BookmarkList.jsx`

**Checkpoint**: All three user stories independently functional. Run full backend and frontend test suites. All tests must pass.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that apply across all user stories

- [X] T045 [P] Add custom `TrimmedNonWhitespace` validation attribute to reject whitespace-only `Title` on create and on update; normalize `Notes` (whitespace-only → null) in `BookmarkService` — update `backend/BookmarkManager.Api/DTOs/CreateBookmarkRequest.cs` and `UpdateBookmarkRequest.cs`
- [X] T046 [P] Add loading spinner/disabled state to `BookmarkList` while fetching and to `BookmarkForm` while submitting in `frontend/src/components/BookmarkList/BookmarkList.jsx` and `BookmarkForm/BookmarkForm.jsx`
- [X] T047 [P] Add general error display for 400 validation errors (field-level messages) and unexpected errors in `frontend/src/components/BookmarkForm/BookmarkForm.jsx`
- [X] T048 [P] Run `dotnet test` from `backend/` and `npm test -- --watchAll=false` from `frontend/` and confirm all tests pass with no failures
- [X] T049 Run quickstart.md smoke tests end-to-end: POST bookmark (201), GET list (200 with item), GET single (200), PATCH title (200, other fields unchanged), DELETE (204), POST duplicate URL (409 naming original title), GET deleted (404)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately; T001-T004 can run in parallel after T001/T002 complete
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories; T006-T009, T011 can run in parallel after T005
- **User Story 1 (Phase 3)**: Depends on Phase 2 completion — test tasks T012-T015 run in parallel; implementation is sequential per layer
- **User Story 2 (Phase 4)**: Depends on Phase 2 completion — can start after Foundational even if US1 not complete (but integration needs US1 to create data)
- **User Story 3 (Phase 5)**: Depends on Phase 2 completion — same as US2
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — no dependency on US2/US3
- **US2 (P2)**: Can start after Foundational — repository and service layers extend US1's implementation
- **US3 (P3)**: Can start after Foundational — shares repository and service with US1/US2

### Within Each User Story

1. Test tasks (marked [P]) → written first and confirmed FAILING
2. Repository implementation → depends on interfaces from Foundational
3. Service implementation → depends on repository
4. Controller implementation → depends on service
5. Frontend service → parallel with backend implementation
6. Frontend components → depends on frontend service
7. Integration (App.jsx wiring) → depends on components

---

## Parallel Opportunities

### Phase 3 — US1 Test Tasks (all in parallel)

```
T012: BookmarkServiceTests.cs (backend service tests)
T013: BookmarksControllerTests.cs (backend controller tests)
T014: bookmarkService.test.js (frontend service tests)
T015: Component test files (frontend component tests)
```

### Phase 3 — US1 Frontend Implementation (parallel with backend)

```
T020: bookmarkService.js (frontend service)   ← can start after T014 tests written
T021: BookmarkCard.jsx                         ← can start after T015 tests written
```

### Phase 4/5 — Backend and Frontend Test Writing (parallel within story)

```
T025 + T026: Backend service + controller tests
T027 + T028: Frontend service + component tests
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Write US1 tests (T012-T015) — confirm they FAIL
4. Complete Phase 3: User Story 1 (T016-T024)
5. **STOP and VALIDATE**: Run all tests, smoke test via quickstart.md
6. Demo: create bookmarks, see them in the list, test duplicate rejection

### Incremental Delivery

1. Foundation → US1 → test independently → demo MVP
2. US1 + US2 → test US2 independently → demo update flow
3. US1 + US2 + US3 → test US3 independently → demo delete flow
4. Polish → full test suite green

### Parallel Team Strategy

With two developers after Foundational phase is complete:
- Developer A: US1 backend (T016-T019) + US2 backend (T029-T031)
- Developer B: US1 frontend (T020-T024) + US2 frontend (T032-T034)
- Merge after each user story checkpoint

---

## Notes

- `[P]` tasks work on different files with no dependency on incomplete concurrent tasks
- `[Story]` label maps each task to a user story for traceability to spec.md
- Test tasks MUST be written before their implementation tasks — red before green
- Each checkpoint is a valid demo point; stop and validate before advancing
- Whitespace-only title and notes normalization (T045) can be done during polish without blocking story delivery
- The in-memory database resets on restart — expected behaviour; documented in quickstart.md
