import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
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
})
