import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BookmarkCard from './BookmarkCard'
import * as bookmarkService from '../../services/bookmarkService'

const mockBookmark = {
  id: '123', url: 'https://example.com', title: 'Example',
  tags: ['tech', 'react'], notes: 'A note', isRead: false,
  createdAt: '2026-06-16T10:00:00Z', lastModifiedAt: '2026-06-16T10:00:00Z',
}

describe('BookmarkCard', () => {
  it('displays url, title, tags, notes, isRead badge, and createdAt', () => {
    render(<BookmarkCard bookmark={mockBookmark} onUpdated={() => {}} onDeleted={() => {}} />)

    expect(screen.getByRole('link', { name: /example\.com/i })).toBeInTheDocument()
    expect(screen.getByText('Example')).toBeInTheDocument()
    expect(screen.getByText('tech')).toBeInTheDocument()
    expect(screen.getByText('react')).toBeInTheDocument()
    expect(screen.getByText('A note')).toBeInTheDocument()
    expect(screen.getByText(/unread/i)).toBeInTheDocument()
  })

  // ── Edit mode ────────────────────────────────────────────────────────────

  it('switches to edit mode when Edit button is clicked', async () => {
    render(<BookmarkCard bookmark={mockBookmark} onUpdated={() => {}} onDeleted={() => {}} />)

    await userEvent.click(screen.getByRole('button', { name: /edit/i }))

    expect(screen.getByDisplayValue('Example')).toBeInTheDocument()
  })

  it('calls updateBookmark on Save and exits edit mode', async () => {
    const updated = { ...mockBookmark, title: 'Updated' }
    vi.spyOn(bookmarkService, 'updateBookmark').mockResolvedValue(updated)
    const onUpdated = vi.fn()

    render(<BookmarkCard bookmark={mockBookmark} onUpdated={onUpdated} onDeleted={() => {}} />)

    await userEvent.click(screen.getByRole('button', { name: /edit/i }))

    const titleInput = screen.getByDisplayValue('Example')
    await userEvent.clear(titleInput)
    await userEvent.type(titleInput, 'Updated')
    await userEvent.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() => expect(onUpdated).toHaveBeenCalledWith(updated))
  })

  it('restores original values on Cancel', async () => {
    render(<BookmarkCard bookmark={mockBookmark} onUpdated={() => {}} onDeleted={() => {}} />)

    await userEvent.click(screen.getByRole('button', { name: /edit/i }))

    const titleInput = screen.getByDisplayValue('Example')
    await userEvent.clear(titleInput)
    await userEvent.type(titleInput, 'Changed')
    await userEvent.click(screen.getByRole('button', { name: /cancel/i }))

    expect(screen.getByText('Example')).toBeInTheDocument()
    expect(screen.queryByDisplayValue('Changed')).not.toBeInTheDocument()
  })

  it('displays 409 conflict error in edit mode', async () => {
    const err = new Error('conflict')
    err.status = 409
    err.detail = 'This URL is already saved as "Other Bookmark".'
    vi.spyOn(bookmarkService, 'updateBookmark').mockRejectedValue(err)

    render(<BookmarkCard bookmark={mockBookmark} onUpdated={() => {}} onDeleted={() => {}} />)

    await userEvent.click(screen.getByRole('button', { name: /edit/i }))
    await userEvent.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() =>
      expect(screen.getByText(/already saved as/i)).toBeInTheDocument()
    )
  })

  // ── Delete ────────────────────────────────────────────────────────────────

  it('shows a Delete button', () => {
    render(<BookmarkCard bookmark={mockBookmark} onUpdated={() => {}} onDeleted={() => {}} />)

    expect(screen.getByRole('button', { name: /delete/i })).toBeInTheDocument()
  })

  it('calls deleteBookmark and onDeleted after confirmation', async () => {
    vi.spyOn(bookmarkService, 'deleteBookmark').mockResolvedValue(undefined)
    const onDeleted = vi.fn()

    render(<BookmarkCard bookmark={mockBookmark} onUpdated={() => {}} onDeleted={onDeleted} />)

    await userEvent.click(screen.getByRole('button', { name: /delete/i }))

    const confirmBtn = await screen.findByRole('button', { name: /confirm/i })
    await userEvent.click(confirmBtn)

    await waitFor(() => expect(onDeleted).toHaveBeenCalledWith('123'))
  })
})
