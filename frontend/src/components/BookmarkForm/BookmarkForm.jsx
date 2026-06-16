import { useState } from 'react'
import { createBookmark } from '../../services/bookmarkService'

const EMPTY = { url: '', title: '', tags: '', notes: '', isRead: false }

export default function BookmarkForm({ onCreated }) {
  const [fields, setFields] = useState(EMPTY)
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  function handleChange(e) {
    const { name, value, type, checked } = e.target
    setFields(f => ({ ...f, [name]: type === 'checkbox' ? checked : value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const payload = {
        url: fields.url,
        title: fields.title,
        tags: fields.tags.split(',').map(t => t.trim()).filter(Boolean),
        notes: fields.notes || null,
        isRead: fields.isRead,
      }
      const created = await createBookmark(payload)
      setFields(EMPTY)
      onCreated(created)
    } catch (err) {
      if (err.status === 400 && err.errors) {
        const msgs = Object.values(err.errors).flat().join(' ')
        setError(msgs)
      } else {
        setError(err.detail || err.message)
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      {error && <p className="error">{error}</p>}
      <div>
        <label htmlFor="url">URL</label>
        <input id="url" name="url" value={fields.url} onChange={handleChange} required />
      </div>
      <div>
        <label htmlFor="title">Title</label>
        <input id="title" name="title" value={fields.title} onChange={handleChange} required />
      </div>
      <div>
        <label htmlFor="tags">Tags</label>
        <input id="tags" name="tags" value={fields.tags} onChange={handleChange} placeholder="comma-separated" />
      </div>
      <div>
        <label htmlFor="notes">Notes</label>
        <textarea id="notes" name="notes" value={fields.notes} onChange={handleChange} />
      </div>
      <div>
        <label htmlFor="isRead">Read</label>
        <input id="isRead" name="isRead" type="checkbox" checked={fields.isRead} onChange={handleChange} />
      </div>
      <button type="submit" disabled={submitting}>Save</button>
    </form>
  )
}
