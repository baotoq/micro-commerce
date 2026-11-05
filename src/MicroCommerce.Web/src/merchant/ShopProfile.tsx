import { useEffect, useState } from 'react'
import { ApiError, getJson, sendJson } from '../api'
import { errorMessage, fieldErrors } from '../lib/forms'
import { useSession } from '../session'
import { ShopForm, type ShopFormValues } from './ShopForm'

type ShopProfileResponse = ShopFormValues & { merchantId: string; description: string | null; version: number }

/** The Merchant edits its Shop name, description and Pickup Address. */
export function ShopProfile() {
  const { token, reloadMe } = useSession()
  const [shop, setShop] = useState<ShopProfileResponse | null>(null)
  const [error, setError] = useState<unknown>(null)
  const [saved, setSaved] = useState(false)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    if (!token) return
    getJson<ShopProfileResponse>('/api/merchant/shop', token).then(setShop, setError)
  }, [token])

  if (!shop) return error ? <p role="alert">{errorMessage(error)}</p> : <p>Loading…</p>

  const save = async (values: ShopFormValues) => {
    setBusy(true)
    setError(null)
    setSaved(false)
    try {
      const updated = await sendJson<ShopProfileResponse>('/api/merchant/shop', {
        token,
        method: 'PUT',
        body: { ...values, description: values.description || null, version: shop.version },
      })
      setShop(updated)
      setSaved(true)
      reloadMe()
    } catch (e) {
      setError(e)
    } finally {
      setBusy(false)
    }
  }

  const message =
    error instanceof ApiError && error.type === 'shop-version-conflict'
      ? 'Your Shop changed elsewhere. Reload the page and try again.'
      : error instanceof ApiError && error.type === 'shop-name-taken'
        ? 'That Shop name is taken. Try another one.'
        : error && !(error instanceof ApiError && error.status === 400)
          ? errorMessage(error)
          : null

  return (
    <section>
      <h3>Shop profile</h3>
      {message && <p role="alert">{message}</p>}
      {saved && <p>Saved.</p>}
      <ShopForm
        key={shop.version}
        initial={{ shopName: shop.shopName, description: shop.description ?? '', pickupAddress: shop.pickupAddress }}
        submitLabel="Save"
        errors={fieldErrors(error)}
        busy={busy}
        onSubmit={(v) => void save(v)}
      />
    </section>
  )
}
