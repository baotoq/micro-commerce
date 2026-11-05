import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router'
import { ApiError, getJson } from '../api'
import { errorMessage } from '../lib/forms'
import { formatPrice } from '../lib/price'
import type { ProductDetail } from './types'

/** A Published Product: images, details, and each Variant's price and Availability (User Stories 3 and 4). */
export function ProductPage() {
  const { id } = useParams()
  const [product, setProduct] = useState<ProductDetail | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [selected, setSelected] = useState<string[]>([])

  useEffect(() => {
    setProduct(null)
    setError(null)
    getJson<ProductDetail>(`/api/marketplace/products/${id}`).then((p) => {
      setProduct(p)
      setSelected(p.options.map((o) => o.values[0] ?? ''))
    }, setError)
  }, [id])

  if (error) return <p role="alert">{error instanceof ApiError && error.status === 404 ? 'This Product is not available.' : errorMessage(error)}</p>
  if (!product) return <p>Loading…</p>

  const variant = product.variants.find((v) => v.optionValues.every((value, i) => value === selected[i]))

  return (
    <article>
      <h2>{product.name}</h2>
      <p>
        Sold by <Link to={`/shops/${product.shop.id}`}>{product.shop.shopName}</Link> · {product.category.name}
      </p>
      {product.images.length === 0 ? (
        <div aria-label="No image" style={{ width: 320, height: 320, background: '#eee' }} />
      ) : (
        <div style={{ display: 'flex', gap: '0.5rem', overflowX: 'auto' }}>
          {product.images.map((image, i) => (
            <img key={image.id} src={image.url} alt={i === 0 ? product.name : ''} width={320} height={320} style={{ objectFit: 'cover' }} />
          ))}
        </div>
      )}
      {product.description && <p>{product.description}</p>}
      {product.options.map((option, i) => (
        <label key={option.name} style={{ display: 'block' }}>
          {option.name}{' '}
          <select value={selected[i]} onChange={(e) => setSelected(selected.map((s, j) => (j === i ? e.target.value : s)))}>
            {option.values.map((value) => (
              <option key={value}>{value}</option>
            ))}
          </select>
        </label>
      ))}
      {variant && (
        <p>
          {formatPrice(variant.price)} · {variant.availability > 0 ? `${variant.availability} available` : 'Sold out'}
        </p>
      )}
      {product.variants.length > 1 && (
        <table>
          <thead>
            <tr>
              <th>Variant</th>
              <th>Price</th>
              <th>Availability</th>
            </tr>
          </thead>
          <tbody>
            {product.variants.map((v) => (
              <tr key={v.id}>
                <td>{v.optionValues.join(' / ')}</td>
                <td>{formatPrice(v.price)}</td>
                <td>{v.availability > 0 ? v.availability : 'Sold out'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </article>
  )
}
