export type OptionDto = { name: string; values: string[] }

export type VariantDto = { id: string; optionValues: string[]; price: number; stock: number; weightGrams: number }

export type ImageRef = { id: string; position: number; url: string }

export type MerchantProduct = {
  id: string
  name: string
  description: string | null
  categoryId: string
  status: 'Draft' | 'Published' | 'Removed'
  options: OptionDto[]
  variants: VariantDto[]
  images: ImageRef[]
  createdAt: string
  version: number
}

export type MerchantProductSummary = {
  id: string
  name: string
  categoryId: string
  status: 'Draft' | 'Published' | 'Removed'
  variantCount: number
  minPrice: number
  maxPrice: number
  totalStock: number
  coverImageUrl: string | null
  createdAt: string
}

export type Page<T> = { items: T[]; page: number; pageSize: number; totalCount: number }

export const conflictMessages: Record<string, string> = {
  'product-version-conflict': 'This Product changed elsewhere. Reload the page and try again.',
  'product-removed': 'A Platform Operator removed this Product, so it can no longer be changed.',
  'image-limit-reached': 'A Product can have at most 9 images.',
}
