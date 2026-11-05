import { useEffect, useState } from 'react'
import { useParams } from 'react-router'
import { ApiError, getJson } from '../api'
import { errorMessage } from '../lib/forms'
import { ProductGrid } from './ProductGrid'

type ShopPageResponse = { id: string; shopName: string; description: string | null }

/** A Merchant's Shop page and its Published Products (User Story 3). */
export function ShopPage() {
  const { id } = useParams()
  const [shop, setShop] = useState<ShopPageResponse | null>(null)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => {
    setShop(null)
    setError(null)
    getJson<ShopPageResponse>(`/api/marketplace/shops/${id}`).then(setShop, setError)
  }, [id])

  if (error) return <p role="alert">{error instanceof ApiError && error.status === 404 ? 'This Shop does not exist.' : errorMessage(error)}</p>
  if (!shop) return <p>Loading…</p>

  return (
    <section>
      <h2>{shop.shopName}</h2>
      {shop.description && <p>{shop.description}</p>}
      <ProductGrid shopId={shop.id} />
    </section>
  )
}
