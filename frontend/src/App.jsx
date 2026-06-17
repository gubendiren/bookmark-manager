import { useState } from 'react'
import BookmarkForm from './components/BookmarkForm/BookmarkForm'
import BookmarkList from './components/BookmarkList/BookmarkList'
import BookmarkSummary from './components/BookmarkSummary/BookmarkSummary'

export default function App() {
  const [refresh, setRefresh] = useState(0)

  function handleCreated() {
    setRefresh(r => r + 1)
  }

  function handleUpdated() {
    setRefresh(r => r + 1)
  }

  function handleDeleted() {
    setRefresh(r => r + 1)
  }

  return (
    <main>
      <h1>Bookmark Manager</h1>
      <BookmarkSummary refresh={refresh} />
      <BookmarkForm onCreated={handleCreated} />
      <BookmarkList refresh={refresh} onUpdated={handleUpdated} onDeleted={handleDeleted} />
    </main>
  )
}
