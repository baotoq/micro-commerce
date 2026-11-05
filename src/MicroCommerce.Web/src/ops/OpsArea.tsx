import { Link, Route, Routes } from 'react-router'
import { useSession } from '../session'
import { CategoryTree } from './CategoryTree'
import { ProductModeration } from './ProductModeration'

/** The Platform Operator area: Categories and Product moderation. */
export function OpsArea() {
  const { token, me } = useSession()

  if (!token) return <p>Sign in as a Platform Operator.</p>
  if (!me) return <p>Loading…</p>
  if (!me.isPlatformOperator) return <p>Only Platform Operators can use this area.</p>

  return (
    <section>
      <h2>Platform operations</h2>
      <nav>
        <Link to="/ops">Categories</Link> · <Link to="/ops/products">Products</Link>
      </nav>
      <Routes>
        <Route index element={<CategoryTree />} />
        <Route path="products" element={<ProductModeration />} />
      </Routes>
    </section>
  )
}
