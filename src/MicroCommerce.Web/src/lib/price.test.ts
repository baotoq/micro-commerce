import { describe, expect, it } from 'vitest'
import { formatPrice, formatPriceRange } from './price'

describe('price', () => {
  it('groups thousands', () => expect(formatPrice(1200000)).toBe('1,200,000'))
  it('shows one price when min equals max', () => expect(formatPriceRange(120000, 120000)).toBe('120,000'))
  it('shows a range when prices differ', () => expect(formatPriceRange(120000, 150000)).toBe('120,000 – 150,000'))
})
