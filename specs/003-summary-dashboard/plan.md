# Implementation Plan: Summary Dashboard

**Branch**: `003-summary-dashboard` | **Date**: 2026-06-17 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-summary-dashboard/spec.md`

## Summary

Add a `GET /api/bookmarks/summary` endpoint that returns aggregate bookmark counts (total, unread, per-tag, untagged). A new `BookmarkSummary` React component fetches this endpoint and displays the counts. The existing `refresh` integer counter in `App.jsx` is extended to also fire on bookmark update and delete, so the summary stays in sync with the bookmark list without a page reload.

## Technical Context

**Language/Version**: C# 12 / .NET 10 (backend), JavaScript with React 18 (frontend)

**Primary Dependencies**:
- Backend: ASP.NET Core 10 Web API, Entity Framework Core 10 In-Memory, xUnit, FluentAssertions, NSubstitute (all existing)
- Frontend: React 18, Fetch API, Vitest, React Testing Library (all existing)

**Storage**: EF Core In-Memory — existing. No schema changes. Summary is computed per-request; never persisted.

**Testing**: xUnit + FluentAssertions + NSubstitute (backend), Vitest + RTL (frontend) — existing toolchain.

**Target Platform**: Local development server — unchanged from prior features.

**Project Type**: Web application — additive extension of existing backend + frontend.

**Performance Goals**: Summary computed and returned within 3 seconds for up to 1,000 bookmarks (in-memory LINQ; no caching required).

**Constraints**: Single user, no auth, in-memory only. Summary is stateless (not persisted).

**Scale/Scope**: Up to 1,000 bookmarks (per constitution Principle V).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|---------|
| I. RESTful API Design | ✅ PASS | New `GET /api/bookmarks/summary` sub-resource; controller handles HTTP concerns only |
| II. Repository Pattern | ✅ PASS | `GetSummaryAsync()` added to `IBookmarkRepository`; service depends on interface, not `DbContext` |
| III. Clean Architecture Layering | ✅ PASS | `BookmarksController` → `BookmarkService` → `IBookmarkRepository`; no layer skips |
| IV. Test-First Development | ✅ PASS | Failing xUnit and Vitest tests written before implementation code |
| V. Simplicity | ✅ PASS | No new entities, no persistence, read-only aggregate — minimum to satisfy FR-001–FR-008 |

All five gates pass. No Complexity Tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/003-summary-dashboard/
├── plan.md              # This file
├── research.md          # Phase 0 design decisions
├── data-model.md        # Phase 1 DTOs and aggregation rules
├── quickstart.md        # Phase 1 local smoke tests
├── contracts/
│   └── summary.md       # Phase 1 GET /api/bookmarks/summary contract
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
backend/
├── BookmarkManager.Api/
│   ├── Controllers/
│   │   └── BookmarksController.cs         ← add [HttpGet("summary")] action
│   ├── Services/
│   │   ├── IBookmarkService.cs            ← add GetSummaryAsync()
│   │   └── BookmarkService.cs             ← implement GetSummaryAsync()
│   ├── Repositories/
│   │   ├── IBookmarkRepository.cs         ← add GetSummaryAsync()
│   │   └── BookmarkRepository.cs          ← implement LINQ aggregation
│   └── DTOs/
│       ├── TagCount.cs                    ← new: { Tag, Count }
│       └── BookmarkSummaryResponse.cs     ← new: { Total, Unread, Tags, UntaggedCount }
└── BookmarkManager.Api.Tests/
    ├── Services/
    │   └── BookmarkServiceTests.cs        ← append GetSummaryAsync tests
    └── Controllers/
        └── BookmarksControllerTests.cs    ← append GetSummary endpoint tests

frontend/
├── src/
│   ├── components/
│   │   └── BookmarkSummary/
│   │       ├── BookmarkSummary.jsx        ← new: summary panel component
│   │       └── BookmarkSummary.test.jsx   ← new: component tests
│   ├── services/
│   │   ├── bookmarkService.js             ← add getSummary()
│   │   └── bookmarkService.test.js        ← append getSummary() tests
│   └── App.jsx                            ← extend: refresh on update/delete, render BookmarkSummary
└── package.json
```

**Structure Decision**: Web application extension — modifying existing backend and frontend directories. No new top-level directories required.
