# Implementation Plan: Core Bookmark Management

**Branch**: `001-core-bookmark-management` | **Date**: 2026-06-16 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-core-bookmark-management/spec.md`

## Summary

Build a RESTful CRUD API for personal bookmark management using .NET 8 Web API with EF Core
In-Memory persistence, paired with a React frontend. The backend follows a strict three-layer
architecture: BookmarksController → BookmarkService → IBookmarkRepository → BookmarkDbContext.
Users can create bookmarks (URL, title, tags, notes, read status), partially update any field
via PATCH, delete bookmarks, and retrieve a single bookmark or the full list. Duplicate URL
detection rejects attempts and names the conflicting bookmark's title in the error response.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (backend), JavaScript with React 18 (frontend)

**Primary Dependencies**:
- Backend: ASP.NET Core 8 Web API, Entity Framework Core 8 In-Memory, xUnit, FluentAssertions
- Frontend: React 18, Fetch API, Jest, React Testing Library

**Storage**: Entity Framework Core 8 In-Memory (`BookmarkDbContext`) — data does not persist
across application restarts; this is a known and accepted constraint per spec Assumptions.

**Testing**: xUnit + FluentAssertions (backend unit tests); Jest + React Testing Library
(frontend component and service tests)

**Target Platform**: Local development server (backend on `http://localhost:5000`);
web browser (React SPA on `http://localhost:3000`)

**Project Type**: Web application — REST API backend + React SPA frontend

**Performance Goals**: Bookmark save confirmation < 3s end-to-end (SC-001);
duplicate URL error response < 2s (SC-002). Both are comfortably met by in-memory persistence.

**Constraints**: Single user — no authentication, no authorization, no multi-tenancy.
In-memory storage only; data lost on restart. No external dependencies.

**Scale/Scope**: Single user, personal tool — no concurrent-user scalability requirements.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|---------|
| I. RESTful API Design | ✅ PASS | Five standard REST endpoints; correct HTTP verbs and status codes; controllers handle HTTP only |
| II. Repository Pattern | ✅ PASS | `IBookmarkRepository` interface + `BookmarkRepository` implementation; `BookmarkService` depends only on the interface |
| III. Clean Architecture Layering | ✅ PASS | `BookmarksController` → `BookmarkService` → `IBookmarkRepository` → `BookmarkDbContext`; no layer skips |
| IV. Test-First Development | ✅ PASS | xUnit service + controller tests written before implementation; Jest component tests before UI code |
| V. Simplicity | ✅ PASS | No auth, no soft-delete, no multi-user complexity; YAGNI respected throughout |

All five gates pass. No Complexity Tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/001-core-bookmark-management/
├── plan.md              # This file
├── research.md          # Phase 0 design decisions
├── data-model.md        # Phase 1 entity and validation model
├── quickstart.md        # Phase 1 local setup guide
├── contracts/
│   └── bookmarks.md     # Phase 1 REST API contracts
├── checklists/
│   ├── requirements.md  # Spec quality checklist (from /speckit-specify)
│   └── crud.md          # Requirements quality deep-dive (from /speckit-checklist)
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
backend/
├── BookmarkManager.Api/
│   ├── Controllers/
│   │   └── BookmarksController.cs
│   ├── Services/
│   │   ├── IBookmarkService.cs
│   │   └── BookmarkService.cs
│   ├── Repositories/
│   │   ├── IBookmarkRepository.cs
│   │   └── BookmarkRepository.cs
│   ├── Data/
│   │   └── BookmarkDbContext.cs
│   ├── DTOs/
│   │   ├── CreateBookmarkRequest.cs
│   │   ├── UpdateBookmarkRequest.cs
│   │   └── BookmarkResponse.cs
│   └── Program.cs
└── BookmarkManager.Api.Tests/
    ├── Services/
    │   └── BookmarkServiceTests.cs
    └── Controllers/
        └── BookmarksControllerTests.cs

frontend/
├── src/
│   ├── components/
│   │   ├── BookmarkForm/
│   │   │   ├── BookmarkForm.jsx
│   │   │   └── BookmarkForm.test.jsx
│   │   ├── BookmarkList/
│   │   │   ├── BookmarkList.jsx
│   │   │   └── BookmarkList.test.jsx
│   │   └── BookmarkCard/
│   │       ├── BookmarkCard.jsx
│   │       └── BookmarkCard.test.jsx
│   ├── services/
│   │   ├── bookmarkService.js
│   │   └── bookmarkService.test.js
│   └── App.jsx
└── package.json
```

**Structure Decision**: Web application layout. Backend is a .NET 8 Web API solution under
`backend/`; frontend is a React SPA under `frontend/`. Two separate project roots, both in
the repository root.
