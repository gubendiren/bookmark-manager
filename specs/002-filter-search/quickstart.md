# Quickstart: Filtering and Search

**Date**: 2026-06-16
**Branch**: `002-filter-search`

## Prerequisites

Both servers from feature 001 running:

```
# Terminal 1 — backend
cd backend
dotnet run --project BookmarkManager.Api
# Listening on http://localhost:5000

# Terminal 2 — frontend
cd frontend
npm run dev
# Listening on http://localhost:3000
```

> **Note**: The in-memory database resets on each backend restart. Re-seed bookmarks before testing.

---

## Seed Data (paste in order)

```bash
# Bookmark 1 — react, unread
curl -s -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://react.dev/learn/hooks","title":"React Hooks Guide","tags":["react","hooks"],"notes":"Official docs on hooks.","isRead":false}'

# Bookmark 2 — react, read
curl -s -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://react.dev/reference","title":"React API Reference","tags":["react"],"notes":null,"isRead":true}'

# Bookmark 3 — typescript, unread
curl -s -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://typescriptlang.org/docs","title":"TypeScript Handbook","tags":["typescript"],"notes":"Good reference for generics.","isRead":false}'
```

---

## Smoke Tests

### 1. No filter — returns all 3

```bash
curl -s http://localhost:5000/api/bookmarks | jq length
# Expected: 3
```

### 2. Filter by tag

```bash
curl -s "http://localhost:5000/api/bookmarks?tag=react" | jq length
# Expected: 2  (bookmarks 1 and 2)

curl -s "http://localhost:5000/api/bookmarks?tag=React" | jq length
# Expected: 2  (case-insensitive)

curl -s "http://localhost:5000/api/bookmarks?tag=python" | jq length
# Expected: 0  (empty array, no error)
```

### 3. Filter by read status

```bash
curl -s "http://localhost:5000/api/bookmarks?status=unread" | jq length
# Expected: 2  (bookmarks 1 and 3)

curl -s "http://localhost:5000/api/bookmarks?status=read" | jq length
# Expected: 1  (bookmark 2)

curl -s "http://localhost:5000/api/bookmarks?status=all" | jq length
# Expected: 3
```

### 4. Keyword search

```bash
curl -s "http://localhost:5000/api/bookmarks?q=hook" | jq length
# Expected: 1  (bookmark 1: title "React Hooks Guide")

curl -s "http://localhost:5000/api/bookmarks?q=generics" | jq length
# Expected: 1  (bookmark 3: notes "Good reference for generics.")

curl -s "http://localhost:5000/api/bookmarks?q=HOOK" | jq length
# Expected: 1  (case-insensitive)

curl -s "http://localhost:5000/api/bookmarks?q=xyzzy" | jq length
# Expected: 0  (empty array, no error)
```

### 5. Combined filters

```bash
curl -s "http://localhost:5000/api/bookmarks?tag=react&status=unread" | jq length
# Expected: 1  (bookmark 1 only)

curl -s "http://localhost:5000/api/bookmarks?tag=react&status=unread&q=hooks" | jq length
# Expected: 1  (bookmark 1)

curl -s "http://localhost:5000/api/bookmarks?tag=react&status=unread&q=typescript" | jq length
# Expected: 0  (no bookmark is both tagged "react" AND has "typescript" in title/notes)
```

### 6. Validation error

```bash
curl -s "http://localhost:5000/api/bookmarks?status=badvalue" | jq .status
# Expected: 400
```

### 7. Edge cases

```bash
# Empty tag — same as no tag filter
curl -s "http://localhost:5000/api/bookmarks?tag=" | jq length
# Expected: 3

# Whitespace keyword — same as no keyword filter
curl -s "http://localhost:5000/api/bookmarks?q=%20%20" | jq length
# Expected: 3
```

---

## Frontend Verification

1. Open `http://localhost:3000`
2. Seed a few bookmarks using the form
3. Type "react" in the **Tag** field → list narrows within 300 ms (debounce)
4. Select **Unread** in the Status dropdown → list updates immediately
5. Type "hook" in the **Search** field → further narrows
6. Clear all fields → full list restores
