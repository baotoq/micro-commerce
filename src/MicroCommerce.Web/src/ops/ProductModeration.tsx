import { useState, type FormEvent } from 'react'
import { ApiError, getJson, sendJson } from '../api'
import { useAuthImage } from '../lib/authImage'
import { errorMessage } from '../lib/forms'
import { formatPrice } from '../lib/price'
import type { ImageRef, MerchantProduct } from '../merchant/products'
import { useSession } from '../session'

type OperatorProduct = MerchantProduct & { merchantId: string; shopName: string | null }

function Picture({ image, token }: { image: ImageRef; token: string | undefined }) {
  const src = useAuthImage(image.url, token)
  return src ? <img src={src} alt="" width={120} height={120} style={{ objectFit: 'cover' }} /> : null
}

/** Platform Operators look up any Product by ID and remove it from the Marketplace (User Story 7). */
export function ProductModeration() {
  const { token } = useSession()
  const [id, setId] = useState('')
  const [product, setProduct] = useState<OperatorProduct | null>(null)
  const [error, setError] = useState<string | null>(null)

  const lookUp = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setProduct(null)
    try {
      setProduct(await getJson<OperatorProduct>(`/api/ops/products/${id.trim()}`, token))
    } catch (err) {
      setError(err instanceof ApiError && err.status === 404 ? 'No Product has that ID.' : errorMessage(err))
    }
  }

  const remove = async () => {
    if (!product || !window.confirm(`Remove “${product.name}” from the Marketplace? This can’t be undone.`)) return
    try {
      setProduct(await sendJson<OperatorProduct>(`/api/ops/products/${product.id}/remove`, { token }))
    } catch (err) {
      setError(errorMessage(err))
    }
  }

  return (
    <section>
      <h3>Products</h3>
      <form onSubmit={(e) => void lookUp(e)}>
        <input aria-label="Product ID" placeholder="Product ID" value={id} onChange={(e) => setId(e.target.value)} required />
        <button type="submit">Look up</button>
      </form>
      {error && <p role="alert">{error}</p>}
      {product && (
        <article>
          <h4>{product.name}</h4>
          <p>
            Status: {product.status} · Shop: {product.shopName ?? product.merchantId}
          </p>
          {product.description && <p>{product.description}</p>}
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            {product.images.map((image) => (
              <Picture key={image.id} image={image} token={token} />
            ))}
          </div>
          <ul>
            {product.variants.map((v) => (
              <li key={v.id}>
                {v.optionValues.join(' / ') || 'Default'}: {formatPrice(v.price)}, stock {v.stock}
              </li>
            ))}
          </ul>
          {product.status !== 'Removed' && <button onClick={() => void remove()}>Remove from the Marketplace</button>}
        </article>
      )}
    </section>
  )
}
