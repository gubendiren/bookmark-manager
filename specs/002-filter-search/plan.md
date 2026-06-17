# Implementation Plan: Filtering and Search

**Branch**: `002-filter-search` | **Date**: 2026-06-16 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-filter-search/spec.md`

## Summary

Extend the existing `GET /api/bookmarks` endpoint to accept three optional query parameters (`tag`, `status`, `q`) that filter the returned bookmark list. The backend adds a `BookmarkFilterRequest` DTO, a new `GetFilteredAsync` repository method, and threads filter logic through the service and controller layers. The frontend adds a `BookmarkFilter` component with debounced text inputs and an immediate status dropdown; filter state is lifted to `App.jsx` and passed to `BookmarkList`.

## Technical Context

**Language/Version**: C# 12 / .NET 10 (backend), JavaScript with React 18 (frontend)

**Primary Dependencies**:
- Backend: ASP.NET Core 10 Web API, Entity Framework Core 10 In-Memory, xUnit, FluentAssertions, NSubstitute (all existing)
- Frontend: React 18, Fetch API, Vitest, React Testing Library (all existing)

**Storage**: EF Core In-Memory — existing. No schema changes. Filter is evaluated per-request; never persisted.

**Testing**: xUnit + FluentAssertions + NSubstitute (backend), Vitest + RTL (frontend) — existing toolchain.

**Target Platform**: Local development server — unchanged from feature 001.

**Project Type**: Web application — extension of existing backend + frontend.

**Performance Goals**: Filtered list returned within 3 seconds for up to 1,000 bookmarks (easily met by in-memory LINQ; no indexing or caching required at this scale).

**Constraints**: Single user, no auth, in-memory only. Filter is stateless (not persisted between sessions).

**Scale/Scope**: Up to 1,000 bookmarks per constitution Principle V and clarification session.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|---------|
| I. RESTful API Design | ✅ PASS | Filter via GET query parameters on existing resource endpoint; controller handles HTTP concerns only |
| II. Repository Pattern | ✅ PASS | `GetFilteredAsync` added to `IBookmarkRepository`; service depends on interface, not `DbContext` |
| III. Clean Architecture Layering | ✅ PASS | `BookmarksController` → `BookmarkService` → `IBookmarkRepository`; no layer skips |
| IV. Test-First Development | ✅ PASS | Failing xUnit and Vitest tests written before implementation code |
| V. Simplicity | ✅ PASS | No new entities, no persistence, AND-only logic, substring matching only |

All five gates pass. No Complexity Tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/002-filter-search/
├── plan.md              # This file
├── research.md          # Phase 0 design decisions
├── data-model.md        # Phase 1 value object and filter rules
├── quickstart.md        # Phase 1 local smoke tests
├── contracts/
│   └── bookmarks.md     # Phase 1 GET /api/bookmarks filter contract
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
backend/
├── BookmarkManager.Api/
│   ├── Controllers/
│   │   └── BookmarksController.cs         ← update GetAll() to bind [FromQuery]
│   ├── Services/
│   │   ├── IBookmarkService.cs            ← update GetAllAsync signature
│   │   └── BookmarkService.cs             ← implement filter normalisation + delegation
│   ├── Repositories/
│   │   ├── IBookmarkRepository.cs         ← add GetFilteredAsync
│   │   └── BookmarkRepository.cs          ← implement GetFilteredAsync LINQ query
│   └── DTOs/
│       └── BookmarkFilterRequest.cs       ← new: tag?, status?, keyword?
└── BookmarkManager.Api.Tests/
    ├── Services/
    │   └── BookmarkServiceTests.cs        ← append filter tests
    └── Controllers/
        └── BookmarksControllerTests.cs    ← append filter tests

frontend/
├── src/
│   ├── components/
│   │   ├── BookmarkFilter/
│   │   │   ├── BookmarkFilter.jsx         ← new: tag input, status select, keyword input
│   │   │   └── BookmarkFilter.test.jsx    ← new: component tests
│   │   └── BookmarkList/
│   │       ├── BookmarkList.jsx           ← update: accept filter prop, pass to getAll()
│   │       └── BookmarkList.test.jsx      ← append: filter-aware tests
│   ├── services/
│   │   ├── bookmarkService.js             ← update getAll() to accept filter + build query string
│   │   └── bookmarkService.test.js        ← append: getAll(filter) tests
│   └── App.jsx                            ← update: add filter state, render BookmarkFilter
└── package.json
```

**Structure Decision**: Web application extension — modifying existing backend and frontend directories. No new top-level directories required.
