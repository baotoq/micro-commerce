import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { useAuth } from 'react-oidc-context'
import { App } from './App'

vi.mock('react-oidc-context', () => ({ useAuth: vi.fn() }))

type Auth = ReturnType<typeof useAuth>

const signedOut = { isLoading: false, isAuthenticated: false, error: undefined, user: null, signinRedirect: vi.fn(), signoutRedirect: vi.fn() }
const signedIn = { ...signedOut, isAuthenticated: true, user: { access_token: 'token-123' } }

function respond(routes: Record<string, () => Response | Promise<Response>>) {
  vi.stubGlobal(
    'fetch',
    vi.fn((input: RequestInfo | URL) => {
      const handler = routes[String(input)]
      return handler ? Promise.resolve(handler()) : Promise.reject(new TypeError('Failed to fetch'))
    }),
  )
}

const ok = (body?: unknown) => new Response(body === undefined ? 'Healthy' : JSON.stringify(body), { status: 200 })

describe('App', () => {
  beforeEach(() => vi.mocked(useAuth).mockReset())
  afterEach(() => {
    cleanup()
    vi.unstubAllGlobals()
  })

  it('offers Sign in when signed out', async () => {
    vi.mocked(useAuth).mockReturnValue(signedOut as unknown as Auth)
    respond({ '/health': () => ok() })

    render(<App />)

    expect(await screen.findByRole('button', { name: 'Sign in' })).toBeTruthy()
    expect(await screen.findByText('API: reachable')).toBeTruthy()
  })

  it('shows the signed-in Account from /api/me', async () => {
    vi.mocked(useAuth).mockReturnValue(signedIn as unknown as Auth)
    respond({
      '/health': () => ok(),
      '/api/me': () =>
        ok({ accountId: '6f0c4f3e-2b1a-4d6e-9a51-0c8e1d7b2a10', displayName: 'Bea Buyer', email: 'buyer@micro-commerce.test', isPlatformOperator: false }),
    })

    render(<App />)

    expect(await screen.findByText('Bea Buyer')).toBeTruthy()
    expect(screen.getByText('Platform Operator: no')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Sign out' })).toBeTruthy()
    const meCall = vi.mocked(fetch).mock.calls.find(([url]) => url === '/api/me')
    expect(meCall?.[1]?.headers).toEqual({ Authorization: 'Bearer token-123' })
  })

  it('reports an unreachable API', async () => {
    vi.mocked(useAuth).mockReturnValue(signedOut as unknown as Auth)
    respond({})

    render(<App />)

    expect(await screen.findByText('API: unreachable')).toBeTruthy()
  })
})
