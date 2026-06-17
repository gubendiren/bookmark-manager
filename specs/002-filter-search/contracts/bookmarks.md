# API Contracts: Bookmarks — Filtering Extension

- **Base URL**: `http://localhost:5000/api`
- **Date**: 2026-06-16
- **Branch**: `002-filter-search`

This document describes changes to the existing `GET /api/bookmarks` endpoint. All other endpoints (`POST`, `GET /{id}`, `PATCH /{id}`, `DELETE /{id}`) are unchanged.

---

## GET /api/bookmarks *(updated)*

**Purpose**: Retrieve bookmarks, optionally filtered by tag, read status, and/or keyword.

### Query Parameters

All parameters are optional. Omitting a parameter applies no restriction on that dimension.

| Parameter | Type | Description |
|-----------|------|-------------|
| `tag` | `string` | Filter by exact tag (case-insensitive). Empty string = no filter. |
| `status` | `string` | Filter by read status. Accepted values: `read`, `unread`, `all`. Omitted = `all`. |
| `q` | `string` | Keyword search in title and notes (case-insensitive substring). Whitespace-only = no filter. |

### Request Examples

```
GET /api/bookmarks
GET /api/bookmarks?tag=react
GET /api/bookmarks?status=unread
GET /api/bookmarks?q=hooks
GET /api/bookmarks?tag=react&status=unread&q=hooks
```

### Responses

**200 OK** — bookmarks matching all active filters (empty array if none match).

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "url": "https://react.dev/learn/hooks",
    "title": "React Hooks Guide",
    "tags": ["react", "hooks"],
    "notes": "Official docs on hooks.",
    "isRead": false,
    "createdAt": "2026-06-16T10:30:00Z",
    "lastModifiedAt": "2026-06-16T10:30:00Z"
  }
]
```

Order: by `createdAt` ascending (unchanged from existing behaviour).

**400 Bad Request** — invalid `status` value.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "status": ["Invalid status value 'xyz'. Accepted values: read, unread, all."]
  }
}
```

---

## Filter Behaviour Reference

| Scenario | Result |
|----------|--------|
| No parameters | All bookmarks (existing behaviour preserved) |
| `?tag=react` | Bookmarks where at least one tag equals "react" (case-insensitive) |
| `?tag=React` | Same as above — comparison is case-insensitive |
| `?tag=` (empty) | Same as no tag parameter — all bookmarks |
| `?status=unread` | Bookmarks where `isRead == false` |
| `?status=all` | Same as no status parameter — all bookmarks |
| `?status=xyz` | 400 Bad Request |
| `?q=hook` | Bookmarks where title OR notes contains "hook" (case-insensitive) |
| `?q=   ` (whitespace) | Same as no `q` parameter — all bookmarks |
| `?tag=react&status=unread&q=hook` | Intersection of all three filters |
| Any valid filter matching no bookmarks | 200 OK with `[]` |

---

## Frontend Service Contract

`bookmarkService.getAll(filter)` accepts an optional filter object and appends non-empty values as query parameters.

```js
// filter shape
{
  tag: string,      // '' = omit from query string
  status: string,   // 'all' = omit from query string
  keyword: string   // '' = omit from query string
}

// Examples
getAll()                              // → GET /api/bookmarks
getAll({ tag: 'react' })              // → GET /api/bookmarks?tag=react
getAll({ status: 'unread' })          // → GET /api/bookmarks?status=unread
getAll({ tag: 'react', status: 'unread', keyword: 'hooks' })
                                      // → GET /api/bookmarks?tag=react&status=unread&q=hooks
```

Backward-compatible: existing callers passing no arguments continue to work.
