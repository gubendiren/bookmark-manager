import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BookmarkList from './BookmarkList'
import * as bookmarkService from '../../services/bookmarkService'

const mockBookmarks = [
  {
    id: '1', url: 'https://a.com', title: 'A', tags: ['tech'], notes: null,
    isRead: false, createdAt: '2026-06-16T10:00:00Z', lastModifiedAt: '2026-06-16T10:00:00Z',
  },
  {
    id: '2', url: 'https://b.com', title: 'B', tags: [], notes: 'note',
    isRead: true, createdAt: '2026-06-16T11:00:00Z', lastModifiedAt: '2026-06-16T11:00:00Z',
  },
]

describe('BookmarkList', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('renders a BookmarkCard for each bookmark on load', async () => {
    vi.spyOn(bookmarkService, 'getAll').mockResolvedValue(mockBookmarks)

    render(<BookmarkList />)

    await waitFor(() => {
      expect(screen.getByText('A')).toBeInTheDocument()
      expect(screen.getByText('B')).toBeInTheDocument()
    })
  })

  it('shows "No bookmarks yet" when list is empty', async () => {
    vi.spyOn(bookmarkService, 'getAll').mockResolvedValue([])

    render(<BookmarkList />)

    await waitFor(() =>
      expect(screen.getByText(/no bookmarks yet/i)).toBeInTheDocument()
    )
  })

  it('passes filter to bookmarkService.getAll', async () => {
    const filter = { tag: 'react', status: 'unread', keyword: 'hooks' }
    const spy = vi.spyOn(bookmarkService, 'getAll').mockResolvedValue([])

    render(<BookmarkList filter={filter} />)

    await waitFor(() => expect(spy).toHaveBeenCalledWith(filter))
  })

  it('re-fetches with filter when filter prop changes', async () => {
    const spy = vi.spyOn(bookmarkService, 'getAll').mockResolvedValue([])
    const { rerender } = render(<BookmarkList filter={{ tag: '', status: 'all', keyword: '' }} />)

    await waitFor(() => expect(spy).toHaveBeenCalledTimes(1))

    rerender(<BookmarkList filter={{ tag: 'react', status: 'all', keyword: '' }} />)

    await waitFor(() => expect(spy).toHaveBeenCalledTimes(2))
  })
})

// ── US3: Mutation callbacks ─────────────────────────────────────────────────

describe('BookmarkList mutation callbacks', () => {
  it('calls onUpdated prop when a bookmark is saved via BookmarkCard', async () => {
    const updatedBookmark = { ...mockBookmarks[0], title: 'Updated A' }
    vi.spyOn(bookmarkService, 'getAll').mockResolvedValue(mockBookmarks)
    vi.spyOn(bookmarkService, 'updateBookmark').mockResolvedValue(updatedBookmark)
    const onUpdated = vi.fn()

    render(<BookmarkList refresh={0} onUpdated={onUpdated} onDeleted={vi.fn()} />)

    await waitFor(() => expect(screen.getByText('A')).toBeInTheDocument())

    await userEvent.click(screen.getAllByRole('button', { name: /edit/i })[0])
    await userEvent.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() => expect(onUpdated).toHaveBeenCalled())
  })

  it('calls onDeleted prop when a bookmark is deleted via BookmarkCard', async () => {
    vi.spyOn(bookmarkService, 'getAll').mockResolvedValue(mockBookmarks)
    vi.spyOn(bookmarkService, 'deleteBookmark').mockResolvedValue(undefined)
    const onDeleted = vi.fn()

    render(<BookmarkList refresh={0} onUpdated={vi.fn()} onDeleted={onDeleted} />)

    await waitFor(() => expect(screen.getByText('A')).toBeInTheDocument())

    await userEvent.click(screen.getAllByRole('button', { name: /delete/i })[0])
    await userEvent.click(screen.getByRole('button', { name: /confirm/i }))

    await waitFor(() => expect(onDeleted).toHaveBeenCalled())
  })
})
