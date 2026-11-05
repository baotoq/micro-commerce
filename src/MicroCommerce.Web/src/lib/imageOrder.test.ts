import { describe, expect, it } from 'vitest'
import { moveImage } from './imageOrder'

describe('moveImage', () => {
  it('moves the third image to the front', () => expect(moveImage(['a', 'b', 'c'], 2, 0)).toEqual(['c', 'a', 'b']))
  it('moves an image one place right', () => expect(moveImage(['a', 'b', 'c'], 0, 1)).toEqual(['b', 'a', 'c']))
  it('clamps moves past either end', () => {
    expect(moveImage(['a', 'b'], 0, -1)).toEqual(['a', 'b'])
    expect(moveImage(['a', 'b'], 1, 5)).toEqual(['a', 'b'])
  })
  it('ignores an unknown index', () => expect(moveImage(['a'], 3, 0)).toEqual(['a']))
})
