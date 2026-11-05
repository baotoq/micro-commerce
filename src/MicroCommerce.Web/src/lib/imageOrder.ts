/** The ids with the item at `from` moved to `to` (both clamped to the list). */
export function moveImage<T>(ids: T[], from: number, to: number): T[] {
  if (from < 0 || from >= ids.length) return ids
  const target = Math.max(0, Math.min(ids.length - 1, to))
  if (target === from) return ids
  const next = [...ids]
  const [moved] = next.splice(from, 1)
  next.splice(target, 0, moved as T)
  return next
}
