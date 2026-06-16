# Quickstart: Core Bookmark Management

**Date**: 2026-06-16
**Branch**: `001-core-bookmark-management`

---

## Prerequisites

| Tool | Minimum Version | Check |
|------|----------------|-------|
| .NET SDK | 8.0 | `dotnet --version` |
| Node.js | 18.x | `node --version` |
| npm | 9.x | `npm --version` |

---

## 1. Clone and Navigate

```bash
git clone <repository-url>
cd bookmark-manager
git checkout 001-core-bookmark-management
```

---

## 2. Run the Backend

```bash
cd backend/BookmarkManager.Api
dotnet restore
dotnet run
```

API starts on `http://localhost:5000`.
Swagger UI available at `http://localhost:5000/swagger` (development mode).

**Run backend tests** (write tests before running this):

```bash
cd backend/BookmarkManager.Api.Tests
dotnet test
```

---

## 3. Run the Frontend

Open a second terminal:

```bash
cd frontend
npm install
npm start
```

React dev server starts on `http://localhost:3000`.
The app proxies API requests to `http://localhost:5000/api`.

**Run frontend tests** (write tests before running this):

```bash
cd frontend
npm test
```

---

## 4. Smoke Test the API

With the backend running, verify core operations:

**Create a bookmark**:
```bash
curl -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com","title":"Example","tags":["test"],"isRead":false}'
```
Expected: `201 Created` with bookmark JSON including `id`.

**List all bookmarks**:
```bash
curl http://localhost:5000/api/bookmarks
```
Expected: `200 OK` with array containing the bookmark above.

**Mark as read (partial update)**:
```bash
curl -X PATCH http://localhost:5000/api/bookmarks/{id} \
  -H "Content-Type: application/json" \
  -d '{"isRead":true}'
```
Expected: `200 OK` with `isRead: true`; other fields unchanged.

**Attempt duplicate URL**:
```bash
curl -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com","title":"Duplicate"}'
```
Expected: `409 Conflict` with `detail` naming "Example".

**Delete bookmark**:
```bash
curl -X DELETE http://localhost:5000/api/bookmarks/{id}
```
Expected: `204 No Content`.

---

## 5. Key Notes

- **Data does not persist** across backend restarts (in-memory store by design).
- The backend must be running before starting the frontend.
- No authentication is required; the API is open by design for this single-user build.
- CORS is pre-configured to allow `http://localhost:3000`.
