import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react'
import { useAuth } from 'react-oidc-context'
import { getMe, type Me } from './api'

type Session = {
  /** The access token while signed in. */
  token: string | undefined
  /** The signed-in Account from GET /api/me, once loaded. */
  me: Me | null
  meError: string | null
  /** Reloads /api/me, e.g. after opening a Merchant. */
  reloadMe: () => void
}

const SessionContext = createContext<Session>({ token: undefined, me: null, meError: null, reloadMe: () => {} })

export function SessionProvider({ children }: { children: ReactNode }) {
  const auth = useAuth()
  const token = auth.isAuthenticated ? auth.user?.access_token : undefined
  const [me, setMe] = useState<Me | null>(null)
  const [meError, setMeError] = useState<string | null>(null)
  const [version, setVersion] = useState(0)

  useEffect(() => {
    if (!token) {
      setMe(null)
      return
    }
    let cancelled = false
    setMeError(null)
    getMe(token)
      .then((result) => !cancelled && setMe(result))
      .catch((e: unknown) => !cancelled && setMeError(e instanceof Error ? e.message : String(e)))
    return () => {
      cancelled = true
    }
  }, [token, version])

  const reloadMe = useCallback(() => setVersion((v) => v + 1), [])

  return <SessionContext.Provider value={{ token, me, meError, reloadMe }}>{children}</SessionContext.Provider>
}

export function useSession(): Session {
  return useContext(SessionContext)
}
