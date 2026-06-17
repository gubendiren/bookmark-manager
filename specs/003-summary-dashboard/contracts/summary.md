# API Contracts: Bookmark Summary

- **Base URL**: `http://localhost:5000/api`
- **Date**: 2026-06-17
- **Branch**: `003-summary-dashboard`

This document describes the new `GET /api/bookmarks/summary` endpoint. All existing bookmark endpoints are unchanged.

---

## GET /api/bookmarks/summary *(new)*

**Purpose**: Retrieve aggregate statistics for the entire bookmark collection — total count, unread count, per-tag counts, and untagged count.

### Request

No parameters. No request body.

```
GET /api/bookmarks/summary
```

### Response

**200 OK** — always returned (even for an empty collection).

```json
{
  "total": 10,
  "unread": 4,
  "tags": [
    { "tag": "design", "count": 3 },
    { "tag": "react", "count": 5 },
    { "tag": "typescript", "count": 2 }
  ],
  "untaggedCount": 1
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `total` | `integer` | Total number of bookmarks stored |
| `unread` | `integer` | Number of bookmarks with `isRead == false` |
| `tags` | `array<TagCount>` | Per-tag breakdown, sorted alphabetically by `tag` |
| `untaggedCount` | `integer` | Number of bookmarks with no tags assigned |

#### TagCount Object

| Field | Type | Description |
|-------|------|-------------|
| `tag` | `string` | Tag label (case-sensitive, as stored) |
| `count` | `integer` | Number of bookmarks assigned to this tag |

#### Empty Collection

```json
{
  "total": 0,
  "unread": 0,
  "tags": [],
  "untaggedCount": 0
}
```

---

## Aggregation Semantics

| Scenario | Behaviour |
|----------|-----------|
| Bookmark with tags `["react","hooks"]` | Contributes 1 to `react` count AND 1 to `hooks` count |
| Bookmark with no tags | Contributes 1 to `untaggedCount`; NOT included in `tags` array |
| Bookmark with `isRead: false` | Contributes 1 to `unread` |
| Tags `"React"` and `"react"` | Counted separately (case-sensitive exact match) |
| `tags` array order | Alphabetically ascending by tag name |
| No bookmarks | `{ "total":0, "unread":0, "tags":[], "untaggedCount":0 }` |

---

## Frontend Service Contract

New function added to `bookmarkService.js`:

```js
export async function getSummary() {
  const res = await fetch('/api/bookmarks/summary')
  return handleResponse(res)
}
```

Returns the summary object directly. No parameters.

```js
// Returned shape
{
  total: number,
  unread: number,
  tags: Array<{ tag: string, count: number }>,
  untaggedCount: number
}
```

---

## Routing Note

`GET /api/bookmarks/summary` is served by a new `[HttpGet("summary")]` action in `BookmarksController`. The existing `[HttpGet("{id:guid}")]` action uses a GUID constraint, so the literal segment "summary" will never match it — no routing conflict.
