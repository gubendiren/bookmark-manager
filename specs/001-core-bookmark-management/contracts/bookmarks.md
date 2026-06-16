# API Contracts: Bookmarks

**Base URL**: `http://localhost:5000/api`
**Date**: 2026-06-16
**Branch**: `001-core-bookmark-management`

All request and response bodies use `application/json`.
Error responses use `application/problem+json` (RFC 7807 ProblemDetails).
All timestamps are ISO 8601 UTC strings (e.g., `"2026-06-16T10:30:00Z"`).

---

## POST /api/bookmarks

**Purpose**: Create a new bookmark.

### Request Body

```json
{
  "url": "https://example.com/article",
  "title": "Example Article",
  "tags": ["reading", "tech"],
  "notes": "Good reference for later.",
  "isRead": false
}
```

| Field | Required | Notes |
|-------|----------|-------|
| `url` | Yes | Must begin with `http://` or `https://`; max 2048 chars |
| `title` | Yes | Non-empty; max 500 chars |
| `tags` | No | Defaults to `[]`; duplicates removed |
| `notes` | No | Defaults to `null` |
| `isRead` | No | Defaults to `false` |

### Responses

**201 Created** — bookmark saved successfully.

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "url": "https://example.com/article",
  "title": "Example Article",
  "tags": ["reading", "tech"],
  "notes": "Good reference for later.",
  "isRead": false,
  "createdAt": "2026-06-16T10:30:00Z",
  "lastModifiedAt": "2026-06-16T10:30:00Z"
}
```

`Location` header set to `/api/bookmarks/{id}`.

**400 Bad Request** — validation failure (missing or invalid fields).

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "url": ["The URL field is required."],
    "title": ["The Title field is required."]
  }
}
```

**409 Conflict** — URL already exists.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Duplicate URL",
  "status": 409,
  "detail": "This URL is already saved as \"Example Article\".",
  "conflictingBookmark": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Example Article"
  }
}
```

---

## GET /api/bookmarks

**Purpose**: Retrieve all saved bookmarks.

### Request

No body. No required parameters.

### Responses

**200 OK** — returns array (empty array if no bookmarks exist).

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "url": "https://example.com/article",
    "title": "Example Article",
    "tags": ["reading", "tech"],
    "notes": "Good reference for later.",
    "isRead": false,
    "createdAt": "2026-06-16T10:30:00Z",
    "lastModifiedAt": "2026-06-16T10:30:00Z"
  }
]
```

Order: by `createdAt` ascending (oldest first).

---

## GET /api/bookmarks/{id}

**Purpose**: Retrieve a single bookmark by its identifier.

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | UUID string | The bookmark's unique identifier |

### Responses

**200 OK** — bookmark found.

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "url": "https://example.com/article",
  "title": "Example Article",
  "tags": ["reading", "tech"],
  "notes": "Good reference for later.",
  "isRead": false,
  "createdAt": "2026-06-16T10:30:00Z",
  "lastModifiedAt": "2026-06-16T10:30:00Z"
}
```

**404 Not Found** — no bookmark with this ID.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Bookmark '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found."
}
```

---

## PATCH /api/bookmarks/{id}

**Purpose**: Partially update one or more fields of an existing bookmark.
Fields omitted from the request body remain unchanged.

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | UUID string | The bookmark's unique identifier |

### Request Body

Only include fields you wish to change.

```json
{
  "title": "Updated Title",
  "isRead": true
}
```

**Tags field behaviour** (only applies when `tags` is included):

| Value | Effect |
|-------|--------|
| Field omitted | Tags unchanged |
| `"tags": null` | Tags unchanged |
| `"tags": []` | All tags cleared |
| `"tags": ["work"]` | Tags replaced with `["work"]` |

### Responses

**200 OK** — update applied; returns the full updated bookmark.

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "url": "https://example.com/article",
  "title": "Updated Title",
  "tags": ["reading", "tech"],
  "notes": "Good reference for later.",
  "isRead": true,
  "createdAt": "2026-06-16T10:30:00Z",
  "lastModifiedAt": "2026-06-16T11:00:00Z"
}
```

**400 Bad Request** — provided field value is invalid (e.g., malformed URL).

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "url": ["URL must begin with http:// or https://."]
  }
}
```

**404 Not Found** — no bookmark with this ID.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Bookmark '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found."
}
```

**409 Conflict** — new URL duplicates a different bookmark's URL.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Duplicate URL",
  "status": 409,
  "detail": "This URL is already saved as \"Other Bookmark\".",
  "conflictingBookmark": {
    "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "title": "Other Bookmark"
  }
}
```

---

## DELETE /api/bookmarks/{id}

**Purpose**: Permanently delete a bookmark by its identifier.

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | UUID string | The bookmark's unique identifier |

### Responses

**204 No Content** — bookmark deleted successfully. No response body.

**404 Not Found** — no bookmark with this ID (including already-deleted bookmarks).

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Bookmark '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found."
}
```

---

## HTTP Status Code Summary

| Status | Meaning | Used by |
|--------|---------|---------|
| 200 OK | Success with body | GET (single + list), PATCH |
| 201 Created | Resource created | POST |
| 204 No Content | Success without body | DELETE |
| 400 Bad Request | Validation failure | POST, PATCH |
| 404 Not Found | Resource not found | GET (single), PATCH, DELETE |
| 409 Conflict | Duplicate URL | POST, PATCH |

## CORS

Backend must allow requests from `http://localhost:3000` (React dev server).
Configure with `AllowAnyHeader`, `AllowAnyMethod` for local development.
