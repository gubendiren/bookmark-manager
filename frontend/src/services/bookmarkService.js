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

export async function getAll(filter) {
  const params = new URLSearchParams()
  if (filter) {
    if (filter.tag) params.set('tag', filter.tag)
    if (filter.status && filter.status !== 'all') params.set('status', filter.status)
    if (filter.keyword) params.set('q', filter.keyword)
  }
  const query = params.toString()
  const res = await fetch(query ? `/api/bookmarks?${query}` : '/api/bookmarks')
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

export async function getSummary() {
  const res = await fetch('/api/bookmarks/summary')
  return handleResponse(res)
}
