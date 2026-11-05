import { describe, expect, it } from 'vitest'
import { buildVariantGrid, combinations } from './variantGrid'

describe('combinations', () => {
  it('is one empty combination without options', () => expect(combinations([])).toEqual([[]]))

  it('varies the last option fastest', () =>
    expect(
      combinations([
        { name: 'Colour', values: ['Red', 'Blue'] },
        { name: 'Size', values: ['S', 'M'] },
      ]),
    ).toEqual([
      ['Red', 'S'],
      ['Red', 'M'],
      ['Blue', 'S'],
      ['Blue', 'M'],
    ]))
})

describe('buildVariantGrid', () => {
  it('keeps what was entered for surviving combinations', () => {
    const before = buildVariantGrid([{ name: 'Size', values: ['S', 'M', 'L'] }], []).map((row, i) => ({ ...row, price: String(100 + i) }))

    const after = buildVariantGrid([{ name: 'Size', values: ['S', 'L', 'XL'] }], before)

    expect(after.map((r) => [r.optionValues[0], r.price])).toEqual([
      ['S', '100'],
      ['L', '102'],
      ['XL', ''],
    ])
  })

  it('has a single default row without options', () => {
    expect(buildVariantGrid([], [])).toEqual([{ optionValues: [], price: '', stock: '', weightGrams: '' }])
  })

  it('ignores options that have no name or no values yet', () => {
    expect(buildVariantGrid([{ name: '', values: ['Red'] }, { name: 'Size', values: [] }], [])).toHaveLength(1)
  })
})
