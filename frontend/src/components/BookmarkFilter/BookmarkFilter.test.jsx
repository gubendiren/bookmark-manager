import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import BookmarkFilter from './BookmarkFilter'

describe('BookmarkFilter', () => {
  it('renders tag input, status select, and keyword input', () => {
    render(<BookmarkFilter onFilterChange={vi.fn()} />)
    expect(screen.getByRole('textbox', { name: /filter by tag/i })).toBeInTheDocument()
    expect(screen.getByRole('combobox', { name: /filter by status/i })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: /search by keyword/i })).toBeInTheDocument()
  })

  it('calls onFilterChange immediately when status changes', () => {
    const onFilterChange = vi.fn()
    render(<BookmarkFilter onFilterChange={onFilterChange} />)

    const select = screen.getByRole('combobox', { name: /filter by status/i })
    fireEvent.change(select, { target: { value: 'unread' } })

    expect(onFilterChange).toHaveBeenCalledWith({ tag: '', status: 'unread', keyword: '' })
  })

  it('calls onFilterChange with updated tag after 300ms debounce', async () => {
    vi.useFakeTimers()
    const onFilterChange = vi.fn()
    render(<BookmarkFilter onFilterChange={onFilterChange} />)

    const tagInput = screen.getByRole('textbox', { name: /filter by tag/i })
    fireEvent.change(tagInput, { target: { value: 'react' } })

    expect(onFilterChange).not.toHaveBeenCalled()

    await act(async () => { vi.advanceTimersByTime(300) })

    expect(onFilterChange).toHaveBeenCalledWith({ tag: 'react', status: 'all', keyword: '' })
    vi.useRealTimers()
  })

  it('calls onFilterChange with updated keyword after 300ms debounce', async () => {
    vi.useFakeTimers()
    const onFilterChange = vi.fn()
    render(<BookmarkFilter onFilterChange={onFilterChange} />)

    const keywordInput = screen.getByRole('textbox', { name: /search by keyword/i })
    fireEvent.change(keywordInput, { target: { value: 'hooks' } })

    await act(async () => { vi.advanceTimersByTime(300) })

    expect(onFilterChange).toHaveBeenCalledWith({ tag: '', status: 'all', keyword: 'hooks' })
    vi.useRealTimers()
  })
})
