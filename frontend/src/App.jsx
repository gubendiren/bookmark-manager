import { useState } from 'react'
import BookmarkForm from './components/BookmarkForm/BookmarkForm'
import BookmarkList from './components/BookmarkList/BookmarkList'

export default function App() {
  const [refresh, setRefresh] = useState(0)

  function handleCreated() {
    setRefresh(r => r + 1)
  }

  return (
    <main>
      <h1>Bookmark Manager</h1>
      <BookmarkForm onCreated={handleCreated} />
      <BookmarkList refresh={refresh} />
    </main>
  )
}
