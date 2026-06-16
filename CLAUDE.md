# Project: Personal Bookmark Manager

## Development Approach
This project follows Spec-Driven Development using the GitHub Spec Kit.

## Phase Sequence
constitution → specify → clarify → plan → tasks → implement → PR → pipeline → acceptance

All specification artifacts live in the `.spec/` folder and are versioned alongside the code.

## The Core Discipline
When implementation diverges from the specification, correct the specification first.
Then correct or regenerate the code to match. Never fix the code without updating the spec.
The specification is the source of truth, not the output.

## Technology Stack
- Backend: .NET 8 Web API
- Frontend: React
- Data: Entity Framework Core In-Memory
- Testing: xUnit (backend), Jest (frontend)

## Conventions
- One feature branch per feature
- One commit per SDD phase
- PR required before merging to main
- CI pipeline must pass before merge
Adapt the content to you