import { useEffect, useState } from 'react'
import { getAll } from '../../services/bookmarkService'
import BookmarkCard from '../BookmarkCard/BookmarkCard'

export default function BookmarkList({ refresh, filter }) {
  const [bookmarks, setBookmarks] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    getAll(filter)
      .then(setBookmarks)
      .finally(() => setLoading(false))
  }, [refresh, filter])

  function handleUpdated(updated) {
    setBookmarks(bs => bs.map(b => b.id === updated.id ? updated : b))
  }

  function handleDeleted(id) {
    setBookmarks(bs => bs.filter(b => b.id !== id))
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
