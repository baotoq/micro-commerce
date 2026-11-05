export type ShopSummary = { id: string; shopName: string }

export type ProductCard = {
  id: string
  name: string
  categoryId: string
  minPrice: number
  maxPrice: number
  available: boolean
  coverImageUrl: string | null
  shop: ShopSummary
}

export type ProductDetail = {
  id: string
  name: string
  description: string | null
  category: { id: string; name: string }
  options: { name: string; values: string[] }[]
  variants: { id: string; optionValues: string[]; price: number; availability: number }[]
  images: { id: string; url: string }[]
  shop: ShopSummary
}

export type Page<T> = { items: T[]; page: number; pageSize: number; totalCount: number }
