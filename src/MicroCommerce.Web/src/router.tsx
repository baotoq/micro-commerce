import { Route, Routes } from 'react-router'
import { Home } from './marketplace/Home'
import { ProductPage } from './marketplace/ProductPage'
import { ShopPage } from './marketplace/ShopPage'
import { OpenShop } from './merchant/OpenShop'
import { MerchantCentre } from './merchant/MerchantCentre'
import { OpsArea } from './ops/OpsArea'

/** Every page of the web app (research R10). */
export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/products/:id" element={<ProductPage />} />
      <Route path="/shops/:id" element={<ShopPage />} />
      <Route path="/open-shop" element={<OpenShop />} />
      <Route path="/merchant/*" element={<MerchantCentre />} />
      <Route path="/ops/*" element={<OpsArea />} />
      <Route path="*" element={<p>Page not found.</p>} />
    </Routes>
  )
}
