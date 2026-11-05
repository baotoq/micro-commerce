import { useState, type ChangeEvent } from 'react'
import { ApiError, sendJson, uploadFile } from '../api'
import { useAuthImage } from '../lib/authImage'
import { errorMessage } from '../lib/forms'
import { moveImage } from '../lib/imageOrder'
import { useSession } from '../session'
import { conflictMessages, type ImageRef, type MerchantProduct } from './products'

const MAX_IMAGES = 9
const MAX_BYTES = 5 * 1024 * 1024
const ACCEPTED = ['image/jpeg', 'image/png', 'image/webp']

function Thumbnail({ image, token }: { image: ImageRef; token: string | undefined }) {
  const src = useAuthImage(image.url, token)
  return src ? <img src={src} alt="" width={96} height={96} style={{ objectFit: 'cover' }} /> : <div style={{ width: 96, height: 96, background: '#eee' }} />
}

/** Upload, reorder and remove a Product's images; the first is the cover (User Story 5). */
export function ImageManager({ product, onChange }: { product: MerchantProduct; onChange: (p: MerchantProduct) => void }) {
  const { token } = useSession()
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const run = async (action: () => Promise<MerchantProduct>) => {
    setBusy(true)
    setError(null)
    try {
      onChange(await action())
    } catch (e) {
      setError(
        e instanceof ApiError && e.type
          ? (conflictMessages[e.type] ?? errorMessage(e))
          : e instanceof ApiError && e.status === 400
            ? (Object.values(e.errors).flat()[0] ?? errorMessage(e))
            : errorMessage(e),
      )
    } finally {
      setBusy(false)
    }
  }

  const upload = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file) return
    // Quick checks for a friendlier message; the server judges the file's content and decides.
    if (!ACCEPTED.includes(file.type)) return setError('Choose a JPEG, PNG or WebP image.')
    if (file.size > MAX_BYTES) return setError('An image must be at most 5 MB.')
    void run(() => uploadFile<MerchantProduct>(`/api/merchant/products/${product.id}/images`, file, token))
  }

  const move = (from: number, to: number) => {
    const ids = moveImage(
      product.images.map((i) => i.id),
      from,
      to,
    )
    void run(() => sendJson<MerchantProduct>(`/api/merchant/products/${product.id}/images/order`, { token, method: 'PUT', body: { imageIds: ids, version: product.version } }))
  }

  const remove = (image: ImageRef) =>
    void run(() => sendJson<MerchantProduct>(`/api/merchant/products/${product.id}/images/${image.id}?version=${product.version}`, { token, method: 'DELETE' }))

  return (
    <section>
      <h4>Images ({product.images.length} of {MAX_IMAGES})</h4>
      {error && <p role="alert">{error}</p>}
      <ol style={{ display: 'flex', gap: '0.5rem', listStyle: 'none', padding: 0 }}>
        {product.images.map((image, i) => (
          <li key={image.id}>
            <Thumbnail image={image} token={token} />
            <div>
              {i === 0 && <strong>Cover </strong>}
              <button type="button" disabled={busy || i === 0} onClick={() => move(i, i - 1)} aria-label="Move left">
                ←
              </button>
              <button type="button" disabled={busy || i === product.images.length - 1} onClick={() => move(i, i + 1)} aria-label="Move right">
                →
              </button>
              <button type="button" disabled={busy} onClick={() => remove(image)}>
                Delete
              </button>
            </div>
          </li>
        ))}
      </ol>
      {product.images.length < MAX_IMAGES && (
        <label>
          Add an image <input type="file" accept={ACCEPTED.join(',')} disabled={busy} onChange={upload} />
        </label>
      )}
    </section>
  )
}
