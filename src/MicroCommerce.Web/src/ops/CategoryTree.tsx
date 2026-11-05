import { useState } from 'react'
import { ApiError, sendJson } from '../api'
import { errorMessage } from '../lib/forms'
import { useCategoryTree, type CategoryNode } from '../marketplace/categories'
import { useSession } from '../session'

const conflictMessages: Record<string, string> = {
  'category-has-products': 'That Category holds Products, so it can’t gain children or be deleted.',
  'category-has-children': 'That Category has child Categories, so it can’t be deleted.',
  'category-name-taken': 'A sibling Category already has that name.',
}

/** Platform Operators maintain the Marketplace-wide Category tree (User Story 6). */
export function CategoryTree() {
  const { token } = useSession()
  const { tree, reload } = useCategoryTree()
  const [error, setError] = useState<string | null>(null)

  const run = async (action: () => Promise<unknown>) => {
    setError(null)
    try {
      await action()
      reload()
    } catch (e) {
      setError(
        e instanceof ApiError && e.type
          ? (conflictMessages[e.type] ?? errorMessage(e))
          : e instanceof ApiError && e.status === 400
            ? (Object.values(e.errors).flat()[0] ?? errorMessage(e))
            : errorMessage(e),
      )
    }
  }

  const add = (parentId: string | null) => {
    const name = window.prompt(parentId ? 'Name of the new child Category' : 'Name of the new top-level Category')
    if (name) void run(() => sendJson('/api/ops/categories', { token, body: { name, parentId } }))
  }
  const rename = (node: CategoryNode) => {
    const name = window.prompt('New name', node.name)
    if (name && name !== node.name) void run(() => sendJson(`/api/ops/categories/${node.id}`, { token, method: 'PUT', body: { name } }))
  }
  const remove = (node: CategoryNode) => {
    if (window.confirm(`Delete ${node.name}?`)) void run(() => sendJson(`/api/ops/categories/${node.id}`, { token, method: 'DELETE' }))
  }

  const renderNodes = (nodes: CategoryNode[]) => (
    <ul>
      {nodes.map((node) => (
        <li key={node.id}>
          {node.name} <button onClick={() => add(node.id)}>Add child</button> <button onClick={() => rename(node)}>Rename</button>{' '}
          <button onClick={() => remove(node)}>Delete</button>
          {node.children.length > 0 && renderNodes(node.children)}
        </li>
      ))}
    </ul>
  )

  return (
    <section>
      <h3>Categories</h3>
      {error && <p role="alert">{error}</p>}
      <button onClick={() => add(null)}>Add top-level Category</button>
      {renderNodes(tree)}
    </section>
  )
}
