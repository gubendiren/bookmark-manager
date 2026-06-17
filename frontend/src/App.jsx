import { useState } from 'react'
import BookmarkForm from './components/BookmarkForm/BookmarkForm'
import BookmarkFilter from './components/BookmarkFilter/BookmarkFilter'
import BookmarkList from './components/BookmarkList/BookmarkList'

export default function App() {
  const [refresh, setRefresh] = useState(0)
  const [filter, setFilter] = useState({ tag: '', status: 'all', keyword: '' })

  function handleCreated() {
    setRefresh(r => r + 1)
  }

  return (
    <main>
      <h1>Bookmark Manager</h1>
      <BookmarkForm onCreated={handleCreated} />
      <BookmarkFilter onFilterChange={setFilter} />
      <BookmarkList refresh={refresh} filter={filter} />
    </main>
  )
}
