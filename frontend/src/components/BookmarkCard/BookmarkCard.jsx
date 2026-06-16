import { useState } from 'react'
import { updateBookmark, deleteBookmark } from '../../services/bookmarkService'

export default function BookmarkCard({ bookmark, onUpdated, onDeleted }) {
  const [editing, setEditing] = useState(false)
  const [fields, setFields] = useState({
    url: bookmark.url,
    title: bookmark.title,
    tags: bookmark.tags.join(', '),
    notes: bookmark.notes ?? '',
    isRead: bookmark.isRead,
  })
  const [editError, setEditError] = useState(null)
  const [confirmDelete, setConfirmDelete] = useState(false)

  function handleChange(e) {
    const { name, value, type, checked } = e.target
    setFields(f => ({ ...f, [name]: type === 'checkbox' ? checked : value }))
  }

  async function handleSave() {
    setEditError(null)
    try {
      const payload = {
        url: fields.url,
        title: fields.title,
        tags: fields.tags.split(',').map(t => t.trim()).filter(Boolean),
        notes: fields.notes || null,
        isRead: fields.isRead,
      }
      const updated = await updateBookmark(bookmark.id, payload)
      onUpdated(updated)
      setEditing(false)
    } catch (err) {
      setEditError(err.detail || err.message)
    }
  }

  function handleCancel() {
    setFields({
      url: bookmark.url,
      title: bookmark.title,
      tags: bookmark.tags.join(', '),
      notes: bookmark.notes ?? '',
      isRead: bookmark.isRead,
    })
    setEditError(null)
    setEditing(false)
  }

  async function handleConfirmDelete() {
    await deleteBookmark(bookmark.id)
    onDeleted(bookmark.id)
  }

  if (editing) {
    return (
      <div className="bookmark-card editing">
        {editError && <p className="error">{editError}</p>}
        <div>
          <label htmlFor={`url-${bookmark.id}`}>URL</label>
          <input id={`url-${bookmark.id}`} name="url" value={fields.url} onChange={handleChange} />
        </div>
        <div>
          <label htmlFor={`title-${bookmark.id}`}>Title</label>
          <input id={`title-${bookmark.id}`} name="title" value={fields.title} onChange={handleChange} />
        </div>
        <div>
          <label htmlFor={`tags-${bookmark.id}`}>Tags</label>
          <input id={`tags-${bookmark.id}`} name="tags" value={fields.tags} onChange={handleChange} />
        </div>
        <div>
          <label htmlFor={`notes-${bookmark.id}`}>Notes</label>
          <textarea id={`notes-${bookmark.id}`} name="notes" value={fields.notes} onChange={handleChange} />
        </div>
        <div>
          <label htmlFor={`isRead-${bookmark.id}`}>Read</label>
          <input id={`isRead-${bookmark.id}`} name="isRead" type="checkbox" checked={fields.isRead} onChange={handleChange} />
        </div>
        <button onClick={handleSave}>Save</button>
        <button onClick={handleCancel}>Cancel</button>
      </div>
    )
  }

  return (
    <div className="bookmark-card">
      <h3>{bookmark.title}</h3>
      <a href={bookmark.url} target="_blank" rel="noopener noreferrer">{bookmark.url}</a>
      <div className="tags">
        {bookmark.tags.map(tag => <span key={tag} className="tag">{tag}</span>)}
      </div>
      {bookmark.notes && <p>{bookmark.notes}</p>}
      <span className="badge">{bookmark.isRead ? 'Read' : 'Unread'}</span>
      <time>{new Date(bookmark.createdAt).toLocaleDateString()}</time>
      <div className="actions">
        <button onClick={() => setEditing(true)}>Edit</button>
        {confirmDelete ? (
          <>
            <button onClick={handleConfirmDelete}>Confirm</button>
            <button onClick={() => setConfirmDelete(false)}>Cancel</button>
          </>
        ) : (
          <button onClick={() => setConfirmDelete(true)}>Delete</button>
        )}
      </div>
    </div>
  )
}
