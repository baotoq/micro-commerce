export type OptionInput = { name: string; values: string[] }

export type VariantRow = { optionValues: string[]; price: string; stock: string; weightGrams: string }

const key = (values: string[]) => values.map((v) => v.trim().toUpperCase()).join('\u001f')

/** Every combination of the Options' values, the last Option varying fastest; one empty combination without Options. */
export function combinations(options: OptionInput[]): string[][] {
  return options.reduce<string[][]>(
    (acc, option) => acc.flatMap((combo) => option.values.map((value) => [...combo, value])),
    [[]],
  )
}

/**
 * One row per combination, in combination order. Rows already filled in for a surviving
 * combination keep their price, stock and weight; new combinations start empty.
 */
export function buildVariantGrid(options: OptionInput[], existing: VariantRow[]): VariantRow[] {
  const usable = options.filter((o) => o.name.trim() && o.values.length > 0)
  const byKey = new Map(existing.map((row) => [key(row.optionValues), row]))
  return combinations(usable).map((values) => {
    const kept = byKey.get(key(values))
    return kept ? { ...kept, optionValues: values } : { optionValues: values, price: '', stock: '', weightGrams: '' }
  })
}
