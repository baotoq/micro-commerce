/** The signed-in Account, as GET /api/me returns it. */
export type Me = {
  accountId: string
  displayName: string
  email: string | null
  isPlatformOperator: boolean
  /** The Merchant this Account owns, or null (002). */
  merchant?: { merchantId: string; shopName: string } | null
}

/** A failed API call, with the ProblemDetails `type` and per-field validation errors when present. */
export class ApiError extends Error {
  readonly status: number
  readonly type: string | undefined
  readonly errors: Record<string, string[]>

  constructor(status: number, type: string | undefined, title: string | undefined, errors: Record<string, string[]> = {}) {
    super(title ?? `Request failed with ${status}`)
    this.status = status
    this.type = type
    this.errors = errors
  }
}

type RequestOptions = { token?: string; method?: string; body?: unknown }

function authHeaders(token: string | undefined): Record<string, string> {
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function toError(response: Response): Promise<ApiError> {
  try {
    const problem = (await response.json()) as { type?: string; title?: string; errors?: Record<string, string[]> }
    return new ApiError(response.status, problem.type, problem.title, problem.errors ?? {})
  } catch {
    return new ApiError(response.status, undefined, undefined)
  }
}

async function send<T>(path: string, init: RequestInit): Promise<T> {
  const response = await fetch(path, init)
  if (!response.ok) throw await toError(response)
  if (response.status === 204) return undefined as T
  const text = await response.text()
  return (text ? JSON.parse(text) : undefined) as T
}

/** GET a JSON resource; signed in when a token is given. */
export function getJson<T>(path: string, token?: string): Promise<T> {
  return send<T>(path, { headers: authHeaders(token) })
}

/** POST, PUT or DELETE with an optional JSON body. */
export function sendJson<T>(path: string, { token, method = 'POST', body }: RequestOptions): Promise<T> {
  return send<T>(path, {
    method,
    headers: { ...authHeaders(token), ...(body === undefined ? {} : { 'Content-Type': 'application/json' }) },
    body: body === undefined ? undefined : JSON.stringify(body),
  })
}

/** Upload one file as multipart/form-data in the `file` field. */
export function uploadFile<T>(path: string, file: File, token?: string): Promise<T> {
  const form = new FormData()
  form.append('file', file)
  return send<T>(path, { method: 'POST', headers: authHeaders(token), body: form })
}

export async function getMe(token: string): Promise<Me> {
  const response = await fetch('/api/me', { headers: { Authorization: `Bearer ${token}` } })
  if (!response.ok) throw new Error(`GET /api/me failed with ${response.status}`)
  return (await response.json()) as Me
}
