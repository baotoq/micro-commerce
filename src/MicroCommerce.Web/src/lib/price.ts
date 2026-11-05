const formatter = new Intl.NumberFormat('en-US')

/** One price in whole Marketplace currency units. */
export function formatPrice(amount: number): string {
  return formatter.format(amount)
}

/** "120,000", or "120,000 – 150,000" when the Variants' prices differ. */
export function formatPriceRange(min: number, max: number): string {
  return min === max ? formatPrice(min) : `${formatPrice(min)} – ${formatPrice(max)}`
}
