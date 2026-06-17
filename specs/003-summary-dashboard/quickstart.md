# Quickstart: Summary Dashboard

**Date**: 2026-06-17
**Branch**: `003-summary-dashboard`

## Prerequisites

Both servers running:

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
# Bookmark 1 — react, hooks — unread
curl -s -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://react.dev/learn/hooks","title":"React Hooks Guide","tags":["react","hooks"],"notes":"Official docs.","isRead":false}'

# Bookmark 2 — react — read
curl -s -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://react.dev/reference","title":"React API Reference","tags":["react"],"notes":null,"isRead":true}'

# Bookmark 3 — typescript — unread
curl -s -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://typescriptlang.org/docs","title":"TypeScript Handbook","tags":["typescript"],"isRead":false}'

# Bookmark 4 — no tags — unread
curl -s -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com","title":"Untagged Bookmark","tags":[],"isRead":false}'
```

After seeding: 4 bookmarks, 3 unread, 1 untagged.

---

## Smoke Tests

### 1. Full summary

```bash
curl -s http://localhost:5000/api/bookmarks/summary | jq .
# Expected:
# {
#   "total": 4,
#   "unread": 3,
#   "tags": [
#     { "tag": "hooks", "count": 1 },
#     { "tag": "react", "count": 2 },
#     { "tag": "typescript", "count": 1 }
#   ],
#   "untaggedCount": 1
# }
```

### 2. Total count

```bash
curl -s http://localhost:5000/api/bookmarks/summary | jq .total
# Expected: 4
```

### 3. Unread count

```bash
curl -s http://localhost:5000/api/bookmarks/summary | jq .unread
# Expected: 3
```

### 4. Untagged count

```bash
curl -s http://localhost:5000/api/bookmarks/summary | jq .untaggedCount
# Expected: 1
```

### 5. Tags sorted alphabetically

```bash
curl -s http://localhost:5000/api/bookmarks/summary | jq '[.tags[].tag]'
# Expected: ["hooks","react","typescript"]
```

### 6. Multi-tag bookmark counted under each tag

```bash
curl -s http://localhost:5000/api/bookmarks/summary | jq '.tags[] | select(.tag=="react") | .count'
# Expected: 2  (bookmarks 1 and 2 both tagged "react")

curl -s http://localhost:5000/api/bookmarks/summary | jq '.tags[] | select(.tag=="hooks") | .count'
# Expected: 1  (only bookmark 1 tagged "hooks")
```

### 7. Empty collection

```bash
# Restart backend (resets in-memory DB), then immediately:
curl -s http://localhost:5000/api/bookmarks/summary | jq .
# Expected: { "total": 0, "unread": 0, "tags": [], "untaggedCount": 0 }
```

---

## Frontend Verification

1. Open `http://localhost:3000`
2. Verify the **Summary Dashboard** panel is visible, showing: **Total: 4 | Unread: 3** and a tag breakdown with hooks (1), react (2), typescript (1)
3. Add a new bookmark via the form → **Total** and **Unread** increment without a page reload
4. Mark a bookmark as read → **Unread** decrements without a page reload
5. Delete a bookmark → **Total** decrements without a page reload
6. Verify the tag breakdown updates accordingly after each action
