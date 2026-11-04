/** The signed-in Account, as GET /api/me returns it. */
export type Me = {
  accountId: string
  displayName: string
  email: string | null
  isPlatformOperator: boolean
}

export async function getMe(token: string): Promise<Me> {
  const response = await fetch('/api/me', { headers: { Authorization: `Bearer ${token}` } })
  if (!response.ok) throw new Error(`GET /api/me failed with ${response.status}`)
  return (await response.json()) as Me
}
