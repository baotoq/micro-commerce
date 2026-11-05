import { useMemo, useState, type FormEvent } from 'react'
import { flatten, useCategoryTree } from './categories'
import { ProductGrid } from './ProductGrid'

/** The Marketplace home: search, Category filter and every Published Product (User Story 3). */
export function Home() {
  const { tree } = useCategoryTree()
  const categories = useMemo(() => flatten(tree), [tree])
  const [draft, setDraft] = useState('')
  const [query, setQuery] = useState('')
  const [categoryId, setCategoryId] = useState('')

  const search = (e: FormEvent) => {
    e.preventDefault()
    setQuery(draft.trim())
  }

  return (
    <section>
      <form role="search" onSubmit={search}>
        <input type="search" aria-label="Search Products" placeholder="Search Products" value={draft} maxLength={200} onChange={(e) => setDraft(e.target.value)} />
        <select aria-label="Category" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
          <option value="">All Categories</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.path}
            </option>
          ))}
        </select>
        <button type="submit">Search</button>
      </form>
      <ProductGrid query={query} categoryId={categoryId} />
    </section>
  )
}
