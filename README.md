# Hands-On Guide: Spec-Driven Development with GitHub Spec Kit and Claude Code

This guide walks you through the same build described in the article, step by step. You will build a Personal Bookmark Manager using Spec-Driven Development. The application itself is simple by design. The point is to experience the SDD workflow end to end, not to build a complex product.

By the end you will have gone through every SDD phase: constitution, specify, clarify, plan, tasks, implement, PR, pipeline, acceptance, and iteration.

---

## What You Need Before You Start

**Accounts and access**
- A GitHub account
- An Anthropic account with access to Claude Code

**Tools installed on your machine**
- Claude Code (installed via `npm install -g @anthropic-ai/claude-code`)
- Git
- Node.js 18 or later
- .NET 8 SDK

**Familiarity assumed**
- You can run commands in a terminal
- You are comfortable with the concept of a Git branch and a pull request

You do not need prior experience with SDD or AI coding agents. That is what this guide is for.

---

## Step 1: Install Claude Code

Claude Code is Anthropic's command-line coding agent. It runs in your terminal, reads your repository, and operates on your files directly. It is the AI agent that will read your specifications and generate code from them.

1. Install Claude Code globally:

```
npm install -g @anthropic-ai/claude-code
```

2. Authenticate with your Anthropic account:

```
claude login
```

3. Confirm it is working by navigating to any directory and running:

```
claude
```

You should see the Claude Code interactive prompt. Type `exit` to leave.

---

## Step 2: Install the GitHub Spec Kit

The GitHub Spec Kit is the methodology layer. It provides the structured slash commands and phase templates that drive the SDD workflow inside Claude Code.

1. Go to the GitHub Spec Kit repository. Search for `Kirchlive/SpecKit` on GitHub to find it.
2. Read the README before proceeding. The Spec Kit is under active development and setup steps may have been updated since this guide was written.
3. Follow the installation instructions in the README to add the Spec Kit to your Claude Code environment.
4. Confirm the Spec Kit commands are available by opening Claude Code in your project directory and typing `/spec-help` or the equivalent command listed in the README.

**What you are setting up:** The Spec Kit gives Claude Code a structured vocabulary for SDD phases. Without it, you would prompt Claude Code freeform for each phase. With it, each phase has a dedicated command that knows what to produce and in what format.

---

## Step 3: Create Your Repository and Project Structure

1. Create a new GitHub repository called `bookmark-manager` (or any name you prefer). Initialise it with a README.
2. Clone it to your local machine:

```
git clone https://github.com/your-username/bookmark-manager.git
cd bookmark-manager
```

3. Create the following folder structure:

```
bookmark-manager/
  .spec/
  src/
  tests/
  CLAUDE.md
```

The `.spec/` folder is where all your SDD artifacts will live: the constitution, the feature specifications, the plan, and the task list. Keeping them together and versioned in the repository means they travel with the code and are never separated from what they govern.

---

## Step 4: Write the `CLAUDE.md` File

This is the most important setup step. The `CLAUDE.md` file at the root of your repository tells Claude Code how to behave in this project. Without it, every Claude Code session starts without context: it does not know it is operating inside an SDD workflow, which phases to follow, or what conventions the project uses. With it, the workflow becomes automatic from the first prompt.

Create `CLAUDE.md` at the repository root with content along these lines:

```markdown
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
```

Adapt the content to your actual project. The more specific your instructions, the less you will need to guide Claude Code manually during the build.

Commit this file before starting any SDD phases:

```
git add CLAUDE.md
git commit -m "Add CLAUDE.md with SDD workflow instructions"
git push
```

---

## Step 5: Phase 1 — Constitution

Open Claude Code in your project directory:

```
claude
```

Use the Spec Kit constitution command (refer to the Spec Kit README for the exact command). When prompted, provide:

- **Application purpose:** A personal bookmark manager. Users save URLs with a title, tags, notes, and a read or unread status. They can filter bookmarks and view a summary dashboard.
- **Target user:** A single user. No authentication or multi-user requirements for this build.
- **Technology stack:** .NET 8 Web API backend, React frontend, Entity Framework Core In-Memory database.
- **Architectural preferences:** Repository pattern for data access. Clean separation between API controllers and business logic. RESTful API design.

Review the constitution Claude Code produces carefully. You are looking for:
- Decisions that do not reflect your intent
- Implicit assumptions you want to make explicit
- Anything missing that should govern the system

Correct the constitution until it accurately reflects your intentions. Then save it as `.spec/constitution.md` and commit:

```
git add .spec/constitution.md
git commit -m "SDD: Constitution"
git push
```

**What you are producing:** A governing document that every subsequent phase and every code generation will be anchored to. This is the single most important artifact in the build.

---

## Step 6: Phase 2 — Specify Features

Specify features one at a time. Use the Spec Kit specify command for each one.

**Feature 1: Bookmark Management**
Core CRUD operations. A user can create a bookmark with a URL, title, tags, notes, and read status. A user can update any field. A user can delete a bookmark. The system must prevent saving a duplicate URL and must return a clear error when a duplicate is attempted.

**Feature 2: Tagging**
Tags are short text labels attached to a bookmark. A bookmark can have zero or more tags. Tags must be trimmed of leading and trailing whitespace before saving. Tags are case-insensitive for filtering purposes.

**Feature 3: Filtering and Search**
A user can filter bookmarks by tag, by read status, or by a text search across title and notes. Filters can be combined. The system must return an empty list, not an error, when no bookmarks match the filter criteria.

**Feature 4: Summary Dashboard**
A dashboard view showing total bookmarks, total unread bookmarks, and a breakdown of bookmarks by tag. The dashboard must reflect current data without requiring a page reload.

For each feature, review the specification Claude Code produces. Check that it uses precise behavioural language: must, should, must not, out of scope. Correct anything vague or missing. Save each specification to `.spec/` and commit after each one:

```
git add .spec/
git commit -m "SDD: Specify [feature name]"
git push
```

---

## Step 7: Phase 3 — Clarify

The clarify phase is where Claude Code reads your specifications and surfaces the ambiguities you did not notice when writing them.

Use the Spec Kit clarify command. Point it at your `.spec/` folder. Review every question it raises. For each one, decide: is this in scope, out of scope, or something the spec should address explicitly?

Typical clarify questions for this build:
- What happens if a tag is only whitespace after trimming?
- Should the duplicate URL check be case-sensitive?
- What should the dashboard show when there are no bookmarks yet?
- Is the text search a substring match or an exact match?

Answer each question by updating the relevant specification. Do not answer in conversation and leave the spec unchanged. The spec is the record, not the chat history.

Commit when done:

```
git add .spec/
git commit -m "SDD: Clarify"
git push
```

---

## Step 8: Phase 4 — Plan

Use the Spec Kit plan command. Claude Code will generate an implementation plan from the constitution and the specifications.

Review the plan for:
- Architectural decisions that conflict with your constitution
- Gaps the spec implies but the plan does not address
- Anything that does not fit your conventions

Correct where needed. Save as `.spec/plan.md` and commit:

```
git add .spec/plan.md
git commit -m "SDD: Plan"
git push
```

You are doing review work here, not authoring work. Your job is to apply judgement to a solid starting point, not to produce the plan from scratch.

---

## Step 9: Phase 5 — Tasks

Use the Spec Kit tasks command. Claude Code will decompose the plan into dependency-ordered implementation tasks.

Review the task list. Tasks should be granular enough that each one produces a small, reviewable unit of code. If a task feels too large, ask Claude Code to decompose it further.

Save the task list as `.spec/tasks.md` and commit:

```
git add .spec/tasks.md
git commit -m "SDD: Tasks"
git push
```

---

## Step 10: Phase 6 — Implement Task by Task

Create a feature branch before writing any code:

```
git checkout -b feature/bookmark-manager
```

Implement tasks one at a time. For each task:

1. Tell Claude Code which task you are working on and point it at the relevant specification:

```
Implement task: [task name]. Reference .spec/spec.md and .spec/plan.md.
```

2. Review the generated code. Check it matches what the specification says. Do not accept code that diverges from the spec, even if it looks reasonable on its own.

3. If the generated code diverges from the spec, decide whether the spec is wrong or the code is wrong:
   - If the spec is wrong, update the spec first, commit it, then ask Claude Code to regenerate.
   - If the code is wrong, tell Claude Code to correct it with explicit reference to the spec.

4. Run the tests after each task before moving to the next.

**The discipline to hold:** Fix the specification before fixing the code. Every time. This is the habit that makes SDD compound over time. A spec that silently diverges from the implementation is not a spec. It is a historical document.

Commit after each task:

```
git add .
git commit -m "Implement: [task name]"
```

---

## Step 11: Phase 7 — PR, Pipeline, and Acceptance

When all tasks for the feature are complete, push your branch and open a pull request:

```
git push origin feature/bookmark-manager
```

Open a pull request on GitHub. The CI pipeline will run automatically. If you have set up a GitHub Actions workflow with a spec compliance check, it will validate the implementation against the specification. If you have not set one up yet, standard unit and integration tests are sufficient for this build.

Review the PR yourself against the specification, not just against the previous code. Merge when the pipeline passes and the review is complete:

```
git checkout main
git merge feature/bookmark-manager
git push
```

---

## Step 12: Phase 8 — Iterate with a New Feature

Add the Favourites feature to prove the loop repeats cleanly.

**Favourites feature:** A user can mark any bookmark as a favourite. Favourites appear in a dedicated section at the top of the bookmark list. The summary dashboard shows a count of favourite bookmarks. Removing a favourite flag does not delete the bookmark.

Before specifying the new feature, update your existing specification documents to ensure the Favourites feature is consistent with what is already there. If the summary dashboard spec says it shows total bookmarks and total unread bookmarks, it now also needs to mention favourites. Spec maintenance is real work. Budget it.

Then run the full loop: specify, clarify, plan, tasks, implement, PR, pipeline, acceptance.

Create a new branch for this iteration:

```
git checkout -b feature/favourites
```

---

## What to Check When You Are Done

By the end of this build you should have:

- `.spec/constitution.md` accurately describing the system's architecture and constraints
- `.spec/spec.md` with precise behavioural specifications for five features
- `.spec/plan.md` reflecting the implementation approach
- `.spec/tasks.md` with the dependency-ordered task list
- A Git history of approximately twelve commits, one per SDD phase across both iterations
- A working Personal Bookmark Manager application
- A CI pipeline that runs on every PR

If any of these are missing or incomplete, trace back to the phase where the gap occurred. The most common gaps are a constitution that was too thin, specifications that used vague language, and a clarify phase that was skipped.

---

## A Note on the `CLAUDE.md` File and New Team Members

When a new team member joins the project, the first thing they should read is `CLAUDE.md`. It explains the workflow, the phase sequence, and the core discipline. Without it, they will not understand why the `.spec/` folder exists, why corrections go into the spec before the code, or what the commit history is intended to communicate.

The `CLAUDE.md` file is not documentation about the application. It is documentation about how the team works. Treat it as a living document and update it when the workflow evolves.

---

## Where to Go Next

This build is spec-first, the first of three SDD maturity levels. The next level, spec-anchored, means adding structured CI validation that mechanically checks implementation compliance against the specification. That is a meaningful engineering investment beyond what this guide covers.

For deeper reading on SDD methodology and tooling options, start with Birgitta Böckeler's tool comparison on Martin Fowler's site and Hari Krishnan's enterprise adoption analysis on InfoQ.

If you run into gaps between this guide and the current state of the Spec Kit, the Spec Kit repository is the authoritative source. This guide reflects the state of the tooling at the time of writing.