import { useEffect, useState } from 'react'

/**
 * Loads an image that needs the bearer token (Merchant Centre and Platform Operator routes) and
 * returns an object URL for <img src>. An <img> request can't carry the token itself (research R10).
 * The object URL is revoked when the URL or token changes, or the component unmounts.
 */
export function useAuthImage(url: string | null | undefined, token: string | undefined): string | null {
  const [objectUrl, setObjectUrl] = useState<string | null>(null)

  useEffect(() => {
    if (!url || !token) {
      setObjectUrl(null)
      return
    }
    let cancelled = false
    let created: string | null = null
    fetchImageObjectUrl(url, token)
      .then((result) => {
        if (cancelled) {
          URL.revokeObjectURL(result)
          return
        }
        created = result
        setObjectUrl(result)
      })
      .catch(() => !cancelled && setObjectUrl(null))
    return () => {
      cancelled = true
      if (created) URL.revokeObjectURL(created)
    }
  }, [url, token])

  return objectUrl
}

export async function fetchImageObjectUrl(url: string, token: string): Promise<string> {
  const response = await fetch(url, { headers: { Authorization: `Bearer ${token}` } })
  if (!response.ok) throw new Error(`GET ${url} failed with ${response.status}`)
  return URL.createObjectURL(await response.blob())
}
