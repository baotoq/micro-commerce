import { Link, Navigate, Route, Routes } from 'react-router'
import { useSession } from '../session'
import { ProductForm } from './ProductForm'
import { ProductList } from './ProductList'
import { ShopProfile } from './ShopProfile'

/** The Merchant Centre: the signed-in Account's own Shop and Products. */
export function MerchantCentre() {
  const { token, me } = useSession()

  if (!token) return <p>Sign in to use the Merchant Centre.</p>
  if (!me) return <p>Loading…</p>
  if (!me.merchant) return <Navigate to="/open-shop" replace />

  return (
    <section>
      <h2>Merchant Centre: {me.merchant.shopName}</h2>
      <nav>
        <Link to="/merchant">Products</Link> · <Link to="/merchant/shop">Shop profile</Link> ·{' '}
        <Link to={`/shops/${me.merchant.merchantId}`}>View Shop page</Link>
      </nav>
      <Routes>
        <Route index element={<ProductList />} />
        <Route path="products/new" element={<ProductForm />} />
        <Route path="products/:id" element={<ProductForm key="edit" />} />
        <Route path="shop" element={<ShopProfile />} />
      </Routes>
    </section>
  )
}
