import { useState, useRef } from 'react'

export default function BookmarkFilter({ onFilterChange }) {
  const [tag, setTag] = useState('')
  const [status, setStatus] = useState('all')
  const [keyword, setKeyword] = useState('')
  const debounceRef = useRef(null)

  function fireChange(filter) {
    onFilterChange(filter)
  }

  function debouncedFireChange(filter) {
    clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => fireChange(filter), 300)
  }

  function handleTagChange(e) {
    const newTag = e.target.value
    setTag(newTag)
    debouncedFireChange({ tag: newTag, status, keyword })
  }

  function handleStatusChange(e) {
    const newStatus = e.target.value
    setStatus(newStatus)
    fireChange({ tag, status: newStatus, keyword })
  }

  function handleKeywordChange(e) {
    const newKeyword = e.target.value
    setKeyword(newKeyword)
    debouncedFireChange({ tag, status, keyword: newKeyword })
  }

  return (
    <div>
      <input
        type="text"
        placeholder="Filter by tag"
        value={tag}
        onChange={handleTagChange}
        aria-label="Filter by tag"
      />
      <select
        value={status}
        onChange={handleStatusChange}
        aria-label="Filter by status"
      >
        <option value="all">All</option>
        <option value="read">Read</option>
        <option value="unread">Unread</option>
      </select>
      <input
        type="text"
        placeholder="Search by keyword"
        value={keyword}
        onChange={handleKeywordChange}
        aria-label="Search by keyword"
      />
    </div>
  )
}
