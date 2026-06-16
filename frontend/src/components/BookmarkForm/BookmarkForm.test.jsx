import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BookmarkForm from './BookmarkForm'
import * as bookmarkService from '../../services/bookmarkService'

const mockBookmark = {
  id: '123', url: 'https://example.com', title: 'Example',
  tags: [], notes: null, isRead: false,
  createdAt: '2026-06-16T10:00:00Z', lastModifiedAt: '2026-06-16T10:00:00Z',
}

describe('BookmarkForm', () => {
  it('renders URL, title, tags, notes, and isRead fields', () => {
    render(<BookmarkForm onCreated={() => {}} />)

    expect(screen.getByLabelText(/url/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/tags/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/notes/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/read/i)).toBeInTheDocument()
  })

  it('submits form and calls onCreated on success', async () => {
    vi.spyOn(bookmarkService, 'createBookmark').mockResolvedValue(mockBookmark)
    const onCreated = vi.fn()

    render(<BookmarkForm onCreated={onCreated} />)

    await userEvent.type(screen.getByLabelText(/url/i), 'https://example.com')
    await userEvent.type(screen.getByLabelText(/title/i), 'Example')
    await userEvent.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() => expect(onCreated).toHaveBeenCalledWith(mockBookmark))
  })

  it('displays 409 conflict error with the conflicting title', async () => {
    const err = new Error('conflict')
    err.status = 409
    err.detail = 'This URL is already saved as "React Docs".'
    vi.spyOn(bookmarkService, 'createBookmark').mockRejectedValue(err)

    render(<BookmarkForm onCreated={() => {}} />)

    await userEvent.type(screen.getByLabelText(/url/i), 'https://react.dev')
    await userEvent.type(screen.getByLabelText(/title/i), 'React')
    await userEvent.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() =>
      expect(screen.getByText(/already saved as/i)).toBeInTheDocument()
    )
  })
})
