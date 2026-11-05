import { afterEach, describe, expect, it, vi } from 'vitest'
import { fetchImageObjectUrl } from './authImage'

describe('fetchImageObjectUrl', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('fetches with the bearer token and returns an object URL', async () => {
    const blob = new Blob(['png'], { type: 'image/png' })
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(new Response(blob, { status: 200 }))))
    const create = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:image-1')

    const url = await fetchImageObjectUrl('/api/merchant/products/p/images/i', 'token-1')

    expect(url).toBe('blob:image-1')
    expect(vi.mocked(fetch).mock.calls[0]?.[1]).toEqual({ headers: { Authorization: 'Bearer token-1' } })
    expect(create).toHaveBeenCalledOnce()
  })

  it('fails when the image is not found', async () => {
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(new Response(null, { status: 404 }))))

    await expect(fetchImageObjectUrl('/api/merchant/products/p/images/i', 'token-1')).rejects.toThrow('404')
  })
})
