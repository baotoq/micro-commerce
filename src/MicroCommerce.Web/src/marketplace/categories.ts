import { useEffect, useState } from 'react'
import { getJson } from '../api'

export type CategoryNode = { id: string; name: string; children: CategoryNode[] }

export type FlatCategory = { id: string; path: string; isLeaf: boolean }

/** The tree as a list of "Home › Kitchen" paths, depth first. */
export function flatten(nodes: CategoryNode[], prefix = ''): FlatCategory[] {
  return nodes.flatMap((node) => {
    const path = prefix ? `${prefix} › ${node.name}` : node.name
    return [{ id: node.id, path, isLeaf: node.children.length === 0 }, ...flatten(node.children, path)]
  })
}

/** The public Category tree; reload() fetches it again. */
export function useCategoryTree() {
  const [tree, setTree] = useState<CategoryNode[]>([])
  const [version, setVersion] = useState(0)
  useEffect(() => {
    getJson<CategoryNode[]>('/api/marketplace/categories').then(setTree, () => setTree([]))
  }, [version])
  return { tree, reload: () => setVersion((v) => v + 1) }
}
