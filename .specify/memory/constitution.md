<!--
  Sync Impact Report
  ==================
  Version change: (unversioned template) → 1.0.0
  Modified principles: N/A — first population from template
  Added sections:
    - Core Principles (5 principles defined)
    - Technology Stack
    - Development Workflow
    - Governance
  Removed sections: None
  Templates requiring updates:
    ✅ .specify/templates/plan-template.md — Constitution Check section is generic;
       aligns with principles I–V as-is
    ✅ .specify/templates/spec-template.md — No mandatory section changes required;
       compatible as-is
    ✅ .specify/templates/tasks-template.md — Phase/task structure aligns with
       Principles III and IV; no changes required
  Deferred TODOs: None — all placeholders resolved
-->

# Personal Bookmark Manager Constitution

## Core Principles

### I. RESTful API Design

All features exposed to the frontend MUST be implemented as RESTful HTTP endpoints.
Controllers MUST handle only HTTP request/response concerns: routing, status codes,
and serialization. Business rules MUST NOT live in controllers.
Standard HTTP verbs (GET, POST, PUT, PATCH, DELETE) MUST be used, and endpoints MUST
return appropriate status codes. API routes MUST follow a consistent resource-naming
convention (e.g., `/api/bookmarks`, `/api/bookmarks/{id}`).

### II. Repository Pattern

All data access MUST be performed through repository interfaces.
Services and controllers MUST NOT reference `DbContext` or EF Core types directly.
Each entity domain (e.g., `Bookmark`) MUST have a corresponding repository interface
and a concrete implementation registered via dependency injection.
This enforces testability and decouples business logic from persistence details.

### III. Clean Architecture Layering

The application MUST maintain strict layer separation:

- **Controllers** (HTTP) → **Services** (business logic) → **Repositories** (data access) → **DbContext** (persistence)
- No layer may skip another: a controller MUST NOT call a repository directly.
- No upward dependency: services MUST NOT import controller types.
- Each layer MUST be independently testable through interface injection.

### IV. Test-First Development (NON-NEGOTIABLE)

Tests MUST be written before implementation code is written.
The Red-Green-Refactor cycle MUST be followed: write a failing test, implement the
minimum code to pass it, then refactor.
Backend tests MUST use xUnit. Frontend tests MUST use Jest.
A feature is not considered complete until all tests pass.
CI MUST run the full test suite and it MUST pass before any merge is permitted.

### V. Simplicity — Single User, No Authentication

This application serves a single user. Authentication, authorization, multi-tenancy,
and user management are explicitly out of scope for this build.
Features MUST NOT introduce complexity for hypothetical future multi-user scenarios.
YAGNI applies: implement only what the current specification requires.
Any exception MUST be justified in `plan.md` under the Complexity Tracking section.

## Technology Stack

- **Backend**: .NET 8 Web API
- **Frontend**: React
- **Database**: Entity Framework Core In-Memory (no external database required for this build)
- **Backend Testing**: xUnit
- **Frontend Testing**: Jest
- **Data Access**: Repository pattern via interfaces registered with .NET dependency injection

Technology stack changes MUST be treated as a MAJOR constitutional amendment and require
explicit ratification before implementation begins.

## Development Workflow

This project follows Spec-Driven Development (SDD). The mandatory phase sequence is:

**constitution → specify → clarify → plan → tasks → implement → PR → pipeline → acceptance**

Rules:

- One feature branch per feature (branch naming: `###-feature-name`)
- One commit per SDD phase
- A pull request is REQUIRED before merging to `main`
- The CI pipeline MUST pass before any merge is permitted
- When implementation diverges from the specification, the specification MUST be
  corrected first; then code is corrected or regenerated to match
- The specification is the source of truth, not the output
- All specification artifacts MUST live in the `.spec/` folder and be versioned
  alongside the code

## Governance

This constitution supersedes all other practices, conventions, and informal agreements.
Any amendment requires:

1. A documented rationale for the change
2. A version increment following semantic versioning (MAJOR.MINOR.PATCH):
   - MAJOR: Backward-incompatible principle removals or redefinitions
   - MINOR: New principle or section added or materially expanded
   - PATCH: Clarifications, wording fixes, non-semantic refinements
3. Propagation of the change to all dependent templates and guidance files
4. An updated `LAST_AMENDED_DATE`

All PRs and code reviews MUST verify compliance with the principles above.
Complexity that violates Principle III or V MUST be explicitly justified in
`plan.md` under the Complexity Tracking section before implementation begins.

**Version**: 1.0.0 | **Ratified**: 2026-06-16 | **Last Amended**: 2026-06-16
