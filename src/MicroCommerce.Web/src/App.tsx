import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { Link } from 'react-router'
import { AppRoutes } from './router'
import { SessionProvider, useSession } from './session'

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

function Account() {
  const auth = useAuth()
  const { me, meError } = useSession()

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
      {meError && <p role="alert">Couldn't load your Account: {meError}</p>}
      {me && (
        <>
          <dl>
            <dt>Signed in as</dt>
            <dd>{me.displayName}</dd>
            <dt>Account ID</dt>
            <dd>{me.accountId}</dd>
          </dl>
          <p>Platform Operator: {me.isPlatformOperator ? 'yes' : 'no'}</p>
          <nav>
            {me.merchant ? <Link to="/merchant">Merchant Centre</Link> : <Link to="/open-shop">Open your Shop</Link>}
            {me.isPlatformOperator && (
              <>
                {' · '}
                <Link to="/ops">Platform operations</Link>
              </>
            )}
          </nav>
        </>
      )}
      <button onClick={() => void auth.signoutRedirect()}>Sign out</button>
    </div>
  )
}

export function App() {
  const apiStatus = useApiStatus()

  return (
    <SessionProvider>
      <header>
        <h1>
          <Link to="/">MicroCommerce</Link>
        </h1>
        <p>API: {apiStatus}</p>
        <Account />
      </header>
      <main>
        <AppRoutes />
      </main>
    </SessionProvider>
  )
}
