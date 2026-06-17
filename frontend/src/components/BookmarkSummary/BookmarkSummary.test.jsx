import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import BookmarkSummary from './BookmarkSummary'
import * as bookmarkService from '../../services/bookmarkService'

const mockSummary = {
  total: 4,
  unread: 3,
  tags: [
    { tag: 'hooks', count: 1 },
    { tag: 'react', count: 2 },
  ],
  untaggedCount: 1,
}

beforeEach(() => {
  vi.restoreAllMocks()
})

// ── US1: View Bookmark Summary Counts ──────────────────────────────────────

describe('BookmarkSummary — counts', () => {
  it('renders total and unread count', async () => {
    vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue(mockSummary)

    render(<BookmarkSummary refresh={0} />)

    await waitFor(() => {
      expect(screen.getByText(/Total: 4/)).toBeInTheDocument()
      expect(screen.getByText(/Unread: 3/)).toBeInTheDocument()
    })
  })

  it('shows empty state message when total is 0', async () => {
    vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue({
      total: 0, unread: 0, tags: [], untaggedCount: 0,
    })

    render(<BookmarkSummary refresh={0} />)

    await waitFor(() =>
      expect(screen.getByText(/no bookmarks yet/i)).toBeInTheDocument()
    )
  })

  it('shows 0 unread when all bookmarks are read', async () => {
    vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue({
      total: 3, unread: 0, tags: [{ tag: 'react', count: 3 }], untaggedCount: 0,
    })

    render(<BookmarkSummary refresh={0} />)

    await waitFor(() => {
      expect(screen.getByText(/Total: 3/)).toBeInTheDocument()
      expect(screen.getByText(/Unread: 0/)).toBeInTheDocument()
    })
  })
})

// ── US2: View Tag Breakdown ─────────────────────────────────────────────────

describe('BookmarkSummary — tag breakdown', () => {
  it('renders each tag with its count', async () => {
    vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue(mockSummary)

    render(<BookmarkSummary refresh={0} />)

    await waitFor(() => {
      expect(screen.getByText(/react: 2/i)).toBeInTheDocument()
      expect(screen.getByText(/hooks: 1/i)).toBeInTheDocument()
    })
  })

  it('renders untagged count when greater than 0', async () => {
    vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue(mockSummary)

    render(<BookmarkSummary refresh={0} />)

    await waitFor(() =>
      expect(screen.getByText(/Untagged: 1/)).toBeInTheDocument()
    )
  })

  it('hides untagged row when untaggedCount is 0', async () => {
    vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue({
      ...mockSummary, untaggedCount: 0,
    })

    render(<BookmarkSummary refresh={0} />)

    await waitFor(() => expect(screen.queryByText(/Untagged/)).not.toBeInTheDocument())
  })

  it('shows empty tag breakdown message when no bookmarks have tags', async () => {
    vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue({
      total: 2, unread: 1, tags: [], untaggedCount: 2,
    })

    render(<BookmarkSummary refresh={0} />)

    await waitFor(() =>
      expect(screen.getByText(/no tagged bookmarks/i)).toBeInTheDocument()
    )
  })
})

// ── US3: Live Data Synchronisation ─────────────────────────────────────────

describe('BookmarkSummary — live sync', () => {
  it('re-fetches when refresh prop increments', async () => {
    const spy = vi.spyOn(bookmarkService, 'getSummary').mockResolvedValue(mockSummary)

    const { rerender } = render(<BookmarkSummary refresh={0} />)
    await waitFor(() => expect(spy).toHaveBeenCalledTimes(1))

    rerender(<BookmarkSummary refresh={1} />)
    await waitFor(() => expect(spy).toHaveBeenCalledTimes(2))
  })
})
