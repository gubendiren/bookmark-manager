import { useEffect, useState } from 'react'
import { getSummary } from '../../services/bookmarkService'

export default function BookmarkSummary({ refresh }) {
  const [summary, setSummary] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    getSummary()
      .then(setSummary)
      .finally(() => setLoading(false))
  }, [refresh])

  if (loading) return <p>Loading summary...</p>

  if (!summary || summary.total === 0) return <p>No bookmarks yet.</p>

  return (
    <section aria-label="Summary">
      <p>Total: {summary.total} | Unread: {summary.unread}</p>
      {summary.tags.length > 0 ? (
        <ul>
          {summary.tags.map(t => (
            <li key={t.tag}>{t.tag}: {t.count}</li>
          ))}
        </ul>
      ) : (
        <p>No tagged bookmarks.</p>
      )}
      {summary.untaggedCount > 0 && <p>Untagged: {summary.untaggedCount}</p>}
    </section>
  )
}
