import { describe, it, expect, vi, beforeEach } from 'vitest'
import * as bookmarkService from './bookmarkService'

const mockBookmark = {
  id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  url: 'https://example.com',
  title: 'Example',
  tags: ['tech'],
  notes: null,
  isRead: false,
  createdAt: '2026-06-16T10:30:00Z',
  lastModifiedAt: '2026-06-16T10:30:00Z',
}

beforeEach(() => {
  vi.restoreAllMocks()
})

// ── createBookmark ──────────────────────────────────────────────────────────

describe('bookmarkService.createBookmark', () => {
  it('sends POST to /api/bookmarks and returns created bookmark', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      status: 201,
      json: async () => mockBookmark,
    }))

    const data = { url: 'https://example.com', title: 'Example' }
    const result = await bookmarkService.createBookmark(data)

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks', expect.objectContaining({
      method: 'POST',
      headers: expect.objectContaining({ 'Content-Type': 'application/json' }),
      body: JSON.stringify(data),
    }))
    expect(result).toEqual(mockBookmark)
  })

  it('throws on 409 conflict', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: false,
      status: 409,
      json: async () => ({
        detail: 'This URL is already saved as "Example".',
        conflictingBookmark: { id: '123', title: 'Example' },
      }),
    }))

    await expect(bookmarkService.createBookmark({ url: 'https://example.com', title: 'Dup' }))
      .rejects.toMatchObject({ status: 409 })
  })
})

// ── getAll ──────────────────────────────────────────────────────────────────

describe('bookmarkService.getAll', () => {
  it('sends GET to /api/bookmarks and returns array', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => [mockBookmark],
    }))

    const result = await bookmarkService.getAll()

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks')
    expect(result).toHaveLength(1)
  })

  it('with tag filter builds correct query string', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true, status: 200, json: async () => [mockBookmark],
    }))

    await bookmarkService.getAll({ tag: 'react', status: 'all', keyword: '' })

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks?tag=react')
  })

  it('with status filter builds correct query string', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true, status: 200, json: async () => [],
    }))

    await bookmarkService.getAll({ tag: '', status: 'unread', keyword: '' })

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks?status=unread')
  })

  it('with keyword filter appends q param', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true, status: 200, json: async () => [],
    }))

    await bookmarkService.getAll({ tag: '', status: 'all', keyword: 'hooks' })

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks?q=hooks')
  })

  it('with all three filters builds full query string', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true, status: 200, json: async () => [],
    }))

    await bookmarkService.getAll({ tag: 'react', status: 'unread', keyword: 'hooks' })

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks?tag=react&status=unread&q=hooks')
  })

  it('with status=all omits status param', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true, status: 200, json: async () => [],
    }))

    await bookmarkService.getAll({ tag: '', status: 'all', keyword: '' })

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks')
  })

  it('with empty tag omits tag param', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true, status: 200, json: async () => [],
    }))

    await bookmarkService.getAll({ tag: '', status: 'all', keyword: '' })

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks')
  })
})

// ── updateBookmark ──────────────────────────────────────────────────────────

describe('bookmarkService.updateBookmark', () => {
  it('sends PATCH with partial body and returns updated bookmark', async () => {
    const updated = { ...mockBookmark, isRead: true }
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => updated,
    }))

    const result = await bookmarkService.updateBookmark(mockBookmark.id, { isRead: true })

    expect(fetch).toHaveBeenCalledWith(
      `/api/bookmarks/${mockBookmark.id}`,
      expect.objectContaining({ method: 'PATCH', body: JSON.stringify({ isRead: true }) })
    )
    expect(result.isRead).toBe(true)
  })
})

// ── deleteBookmark ──────────────────────────────────────────────────────────

describe('bookmarkService.deleteBookmark', () => {
  it('sends DELETE and resolves on 204', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      status: 204,
    }))

    await expect(bookmarkService.deleteBookmark(mockBookmark.id)).resolves.toBeUndefined()

    expect(fetch).toHaveBeenCalledWith(
      `/api/bookmarks/${mockBookmark.id}`,
      expect.objectContaining({ method: 'DELETE' })
    )
  })
})

// ── getSummary ──────────────────────────────────────────────────────────────

describe('bookmarkService.getSummary', () => {
  it('sends GET to /api/bookmarks/summary and returns summary object', async () => {
    const mockSummary = { total: 4, unread: 3, tags: [], untaggedCount: 1 }
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => mockSummary,
    }))

    const result = await bookmarkService.getSummary()

    expect(fetch).toHaveBeenCalledWith('/api/bookmarks/summary')
    expect(result).toEqual(mockSummary)
  })
})
