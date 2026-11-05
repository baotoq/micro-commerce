import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { getJson } from '../api'
import { errorMessage } from '../lib/forms'
import { formatPriceRange } from '../lib/price'
import type { Page, ProductCard } from './types'

const PAGE_SIZE = 20

/** A paged grid of Product cards for a Marketplace query (search, Category, Shop). */
export function ProductGrid({ query, categoryId, shopId }: { query?: string; categoryId?: string; shopId?: string }) {
  const [page, setPage] = useState(1)
  const [result, setResult] = useState<Page<ProductCard> | null>(null)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => setPage(1), [query, categoryId, shopId])

  useEffect(() => {
    const params = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE) })
    if (query) params.set('query', query)
    if (categoryId) params.set('categoryId', categoryId)
    if (shopId) params.set('shopId', shopId)
    setError(null)
    getJson<Page<ProductCard>>(`/api/marketplace/products?${params}`).then(setResult, setError)
  }, [query, categoryId, shopId, page])

  if (error) return <p role="alert">{errorMessage(error)}</p>
  if (!result) return <p>Loading…</p>
  if (result.items.length === 0) return <p>No Products found.</p>

  const pages = Math.max(1, Math.ceil(result.totalCount / result.pageSize))
  return (
    <>
      <ul style={{ listStyle: 'none', padding: 0, display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(180px, 1fr))', gap: '1rem' }}>
        {result.items.map((p) => (
          <li key={p.id}>
            <Link to={`/products/${p.id}`}>
              {p.coverImageUrl ? (
                <img src={p.coverImageUrl} alt="" width={180} height={180} style={{ objectFit: 'cover' }} />
              ) : (
                <div aria-label="No image" style={{ width: 180, height: 180, background: '#eee' }} />
              )}
              <div>{p.name}</div>
            </Link>
            <div>{formatPriceRange(p.minPrice, p.maxPrice)}</div>
            {!p.available && <div>Sold out</div>}
            <Link to={`/shops/${p.shop.id}`}>{p.shop.shopName}</Link>
          </li>
        ))}
      </ul>
      <nav>
        <button disabled={page <= 1} onClick={() => setPage(page - 1)}>
          Previous
        </button>{' '}
        Page {page} of {pages}{' '}
        <button disabled={page >= pages} onClick={() => setPage(page + 1)}>
          Next
        </button>
      </nav>
    </>
  )
}
