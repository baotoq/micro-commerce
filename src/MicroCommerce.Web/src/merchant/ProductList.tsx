import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router'
import { ApiError, getJson, sendJson } from '../api'
import { useAuthImage } from '../lib/authImage'
import { errorMessage } from '../lib/forms'
import { formatPriceRange } from '../lib/price'
import { useSession } from '../session'
import { conflictMessages, type MerchantProduct, type MerchantProductSummary, type Page } from './products'

function Cover({ url, token }: { url: string | null; token: string | undefined }) {
  const src = useAuthImage(url, token)
  return src ? <img src={src} alt="" width={48} height={48} style={{ objectFit: 'cover' }} /> : <span aria-hidden>▢</span>
}

/** Every one of the Merchant's own Products, in every status (User Story 2). */
export function ProductList() {
  const { token } = useSession()
  const [page, setPage] = useState<Page<MerchantProductSummary> | null>(null)
  const [error, setError] = useState<unknown>(null)

  const load = useCallback(() => {
    if (!token) return
    getJson<Page<MerchantProductSummary>>('/api/merchant/products?pageSize=100', token).then(setPage, setError)
  }, [token])

  useEffect(load, [load])

  const changeStatus = async (product: MerchantProductSummary, action: 'publish' | 'unpublish') => {
    setError(null)
    try {
      // The list has no version; read the Product's current one first.
      const current = await getJson<MerchantProduct>(`/api/merchant/products/${product.id}`, token)
      await sendJson(`/api/merchant/products/${product.id}/${action}`, { token, body: { version: current.version } })
      load()
    } catch (e) {
      setError(e)
    }
  }

  if (!page) return error ? <p role="alert">{errorMessage(error)}</p> : <p>Loading…</p>

  return (
    <section>
      <h3>Products</h3>
      <p>
        <Link to="/merchant/products/new">New Product</Link>
      </p>
      {error !== null && (
        <p role="alert">{error instanceof ApiError && error.type ? (conflictMessages[error.type] ?? errorMessage(error)) : errorMessage(error)}</p>
      )}
      {page.items.length === 0 ? (
        <p>You have no Products yet.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th />
              <th>Name</th>
              <th>Status</th>
              <th>Variants</th>
              <th>Price</th>
              <th>Total stock</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {page.items.map((p) => (
              <tr key={p.id}>
                <td>
                  <Cover url={p.coverImageUrl} token={token} />
                </td>
                <td>
                  <Link to={`/merchant/products/${p.id}`}>{p.name}</Link>
                </td>
                <td>{p.status === 'Removed' ? 'Removed by a Platform Operator' : p.status}</td>
                <td>{p.variantCount}</td>
                <td>{formatPriceRange(p.minPrice, p.maxPrice)}</td>
                <td>{p.totalStock}</td>
                <td>
                  {p.status === 'Draft' && <button onClick={() => void changeStatus(p, 'publish')}>Publish</button>}
                  {p.status === 'Published' && <button onClick={() => void changeStatus(p, 'unpublish')}>Unpublish</button>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
