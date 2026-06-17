import { useEffect, useState } from 'react'
import { getAll } from '../../services/bookmarkService'
import BookmarkCard from '../BookmarkCard/BookmarkCard'

export default function BookmarkList({ refresh, onUpdated, onDeleted }) {
  const [bookmarks, setBookmarks] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    getAll()
      .then(setBookmarks)
      .finally(() => setLoading(false))
  }, [refresh])

  function handleUpdated(updated) {
    setBookmarks(bs => bs.map(b => b.id === updated.id ? updated : b))
    onUpdated?.()
  }

  function handleDeleted(id) {
    setBookmarks(bs => bs.filter(b => b.id !== id))
    onDeleted?.()
  }

  if (loading) return <p>Loading...</p>

  if (bookmarks.length === 0) return <p>No bookmarks yet</p>

  return (
    <ul>
      {bookmarks.map(b => (
        <li key={b.id}>
          <BookmarkCard bookmark={b} onUpdated={handleUpdated} onDeleted={handleDeleted} />
        </li>
      ))}
    </ul>
  )
}
