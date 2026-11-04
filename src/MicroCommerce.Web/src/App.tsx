import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { getMe, type Me } from './api'

type ApiStatus = 'checking' | 'reachable' | 'unreachable'

function useApiStatus(): ApiStatus {
  const [status, setStatus] = useState<ApiStatus>('checking')

  useEffect(() => {
    let cancelled = false
    fetch('/health')
      .then((response) => !cancelled && setStatus(response.ok ? 'reachable' : 'unreachable'))
      .catch(() => !cancelled && setStatus('unreachable'))
    return () => {
      cancelled = true
    }
  }, [])

  return status
}

function useMe(token: string | undefined) {
  const [me, setMe] = useState<Me | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!token) {
      setMe(null)
      return
    }
    let cancelled = false
    setError(null)
    getMe(token)
      .then((result) => !cancelled && setMe(result))
      .catch((e: unknown) => !cancelled && setError(e instanceof Error ? e.message : String(e)))
    return () => {
      cancelled = true
    }
  }, [token])

  return { me, error }
}

function Account() {
  const auth = useAuth()
  const { me, error } = useMe(auth.isAuthenticated ? auth.user?.access_token : undefined)

  if (auth.isLoading) return <p>Checking your session…</p>

  if (auth.error) {
    // Usually Keycloak is still starting (spec edge case): let the Account try again.
    return (
      <div>
        <p role="alert">Sign-in isn't available yet. The identity provider may still be starting.</p>
        <button onClick={() => void auth.signinRedirect()}>Try again</button>
      </div>
    )
  }

  if (!auth.isAuthenticated) {
    return <button onClick={() => void auth.signinRedirect()}>Sign in</button>
  }

  return (
    <div>
      {error && <p role="alert">Couldn't load your Account: {error}</p>}
      {me && (
        <>
          <dl>
            <dt>Signed in as</dt>
            <dd>{me.displayName}</dd>
            <dt>Account ID</dt>
            <dd>{me.accountId}</dd>
          </dl>
          <p>Platform Operator: {me.isPlatformOperator ? 'yes' : 'no'}</p>
        </>
      )}
      <button onClick={() => void auth.signoutRedirect()}>Sign out</button>
    </div>
  )
}

export function App() {
  const apiStatus = useApiStatus()

  return (
    <main>
      <h1>MicroCommerce</h1>
      <p>API: {apiStatus}</p>
      <Account />
    </main>
  )
}
