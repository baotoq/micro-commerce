import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router'
import { ApiError, sendJson } from '../api'
import { errorMessage, fieldErrors } from '../lib/forms'
import { useSession } from '../session'
import { emptyShop, ShopForm, type ShopFormValues } from './ShopForm'

const conflictMessages: Record<string, string> = {
  'merchant-exists': 'You already own a Merchant.',
  'shop-name-taken': 'That Shop name is taken. Try another one.',
}

/** An Account without a Merchant opens one (User Story 1). */
export function OpenShop() {
  const { token, me, reloadMe } = useSession()
  const navigate = useNavigate()
  const [error, setError] = useState<unknown>(null)
  const [busy, setBusy] = useState(false)

  if (!token) return <p>Sign in to open your Shop.</p>
  if (me?.merchant) return <Navigate to="/merchant" replace />

  const open = async (values: ShopFormValues) => {
    setBusy(true)
    setError(null)
    try {
      await sendJson('/api/me/merchant', { token, body: { ...values, description: values.description || null } })
      reloadMe()
      navigate('/merchant')
    } catch (e) {
      setError(e)
    } finally {
      setBusy(false)
    }
  }

  const conflict = error instanceof ApiError && error.type ? conflictMessages[error.type] : undefined

  return (
    <section>
      <h2>Open your Shop</h2>
      {error !== null && !(error instanceof ApiError && error.status === 400) && <p role="alert">{conflict ?? errorMessage(error)}</p>}
      <ShopForm initial={emptyShop} submitLabel="Open your Shop" errors={fieldErrors(error)} busy={busy} onSubmit={(v) => void open(v)} />
    </section>
  )
}
