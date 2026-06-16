async function handleResponse(res) {
  if (res.ok) {
    if (res.status === 204) return undefined
    return res.json()
  }
  const body = await res.json().catch(() => ({}))
  const err = new Error(body.detail || body.title || 'Request failed')
  err.status = res.status
  err.detail = body.detail
  err.conflictingBookmark = body.conflictingBookmark
  err.errors = body.errors
  throw err
}

export async function createBookmark(data) {
  const res = await fetch('/api/bookmarks', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  return handleResponse(res)
}

export async function getAll() {
  const res = await fetch('/api/bookmarks')
  return handleResponse(res)
}

export async function updateBookmark(id, fields) {
  const res = await fetch(`/api/bookmarks/${id}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(fields),
  })
  return handleResponse(res)
}

export async function deleteBookmark(id) {
  const res = await fetch(`/api/bookmarks/${id}`, { method: 'DELETE' })
  return handleResponse(res)
}
