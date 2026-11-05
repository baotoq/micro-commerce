# HTTP API Contract: Product Catalog

All routes are under the existing `/api` prefix. JSON uses camelCase. Errors use the ProblemDetails
format (RFC 9457). Validation errors are `HttpValidationProblemDetails`, with one camelCased entry
per field (001 FR-015 and FR-016).

## Common rules

- IDs are UUID strings. Prices are integers in whole Marketplace currency units. Weights are in
  grams.
- **Auth groups** (research R9):

  | Group | Rule |
  |-------|------|
  | `/me/*` | 401 without a valid token |
  | `/merchant/*` | 401 without a token. 403 (`type: merchant-required`) for an Account without a Merchant. **404 for any Product or image of another Merchant** |
  | `/marketplace/*` | Public (`AllowAnonymous`). Hidden items return 404 or are left out |
  | `/ops/*` | 401 without a token. 403 for an Account without the `platform-operator` role |

- **Conflict types**: each 409 carries a ProblemDetails `type` that the web app branches on.

  | `type` | Meaning |
  |--------|---------|
  | `merchant-exists` | The Account already owns a Merchant |
  | `shop-name-taken` | The Shop name is taken |
  | `product-version-conflict` | The `version` was stale, or a concurrent save won |
  | `shop-version-conflict` | The Shop profile's `version` was stale |
  | `product-removed` | The Product is Removed, so Merchant changes are refused |
  | `category-has-products` | The Category holds Products |
  | `category-has-children` | The Category has children |
  | `category-name-taken` | A sibling has that name |
  | `image-limit-reached` | The Product already has 9 images |

- No request ever carries `merchantId` or `accountId`. Both come from the token (FR-016).

---

## Account and Merchant (`/me`, `/merchant/shop`)

| Method and route | Request | Success | Errors |
|------------------|---------|---------|--------|
| `GET /me` | | 200 `MeResponse` | 401 |
| `POST /me/merchant` | `OpenMerchantRequest` | 201 `ShopProfileResponse`, `Location: /api/marketplace/shops/{id}` | 400, 401, 409 `merchant-exists` or `shop-name-taken` |
| `GET /merchant/shop` | | 200 `ShopProfileResponse` | 401, 403 |
| `PUT /merchant/shop` | `UpdateShopRequest` | 200 `ShopProfileResponse` | 400, 401, 403, 409 `shop-name-taken` or `shop-version-conflict` |

```jsonc
// MeResponse: 001's response plus one additive, nullable field
{ "accountId": "…", "displayName": "…", "email": "…", "isPlatformOperator": false,
  "merchant": { "merchantId": "…", "shopName": "Sunny Mugs" } }   // null when the Account owns no Merchant

// OpenMerchantRequest
{ "shopName": "Sunny Mugs", "description": "Hand-thrown mugs",
  "pickupAddress": { "street": "12 Le Loi", "district": "District 1", "province": "Ho Chi Minh City" } }

// UpdateShopRequest = OpenMerchantRequest + "version": 3
// ShopProfileResponse
{ "merchantId": "…", "shopName": "…", "description": "…", "pickupAddress": { … }, "version": 3 }
```

**Validation** (400):

- `shopName` is required and 3–50 characters after trimming.
- `description` is at most 2,000 characters.
- `pickupAddress.street` is required and at most 200 characters.
- `pickupAddress.district` and `pickupAddress.province` are required and at most 100 characters.

---

## Merchant Centre Products (`/merchant/products`)

| Method and route | Request | Success | Errors |
|------------------|---------|---------|--------|
| `GET /merchant/products?page&pageSize` | | 200 `Page<MerchantProductSummary>`, newest first | 401, 403 |
| `POST /merchant/products` | `ProductRequest` | 201 `MerchantProductResponse`, `Location` | 400 (including a non-leaf or unknown `categoryId`), 401, 403 |
| `GET /merchant/products/{id}` | | 200 `MerchantProductResponse` | 401, 403, 404 |
| `PUT /merchant/products/{id}` | `ProductRequest` + `version` | 200 `MerchantProductResponse` | 400, 401, 403, 404, 409 `product-version-conflict` or `product-removed` |
| `POST /merchant/products/{id}/publish` | `{ "version": n }` | 200 `MerchantProductResponse` | 401, 403, 404, 409 (as for PUT) |
| `POST /merchant/products/{id}/unpublish` | `{ "version": n }` | 200 `MerchantProductResponse` | as for publish |

```jsonc
// ProductRequest
{
  "name": "Ceramic coffee mug",
  "description": "350 ml, dishwasher safe",
  "categoryId": "…",                       // must be a leaf
  "options": [                              // 0–2 options; [] means the single default Variant
    { "name": "Colour", "values": ["Red", "Blue"] },
    { "name": "Size",   "values": ["S", "M", "L"] }
  ],
  "variants": [                             // exactly the cartesian product, in any order
    { "optionValues": ["Red", "S"], "price": 120000, "stock": 10, "weightGrams": 400 }
    // … one entry per combination; with no options, exactly one entry whose optionValues is []
  ]
}

// MerchantProductResponse
{
  "id": "…", "name": "…", "description": "…", "categoryId": "…",
  "status": "Draft",                         // Draft | Published | Removed
  "options": [ … ],
  "variants": [ { "id": "…", "optionValues": ["Red","S"], "price": 120000, "stock": 10, "weightGrams": 400 } ],
  "images":   [ { "id": "…", "position": 0, "url": "/api/merchant/products/{id}/images/{imageId}" } ],
  "createdAt": "…", "version": 4
}

// MerchantProductSummary
{ "id": "…", "name": "…", "categoryId": "…", "status": "Published", "variantCount": 6,
  "minPrice": 120000, "maxPrice": 150000, "totalStock": 60, "coverImageUrl": "…" /* or null */, "createdAt": "…" }
```

**Validation** (400). Each failure names its field:

- `name` is 1–200 characters. `description` is at most 5,000 characters.
- `categoryId` must exist and be a leaf.
- There are at most 2 options. Option names are 1–50 characters and distinct ignoring case. Each
  option has 1–20 values, each 1–50 characters and distinct ignoring case.
- `variants` must cover the combinations exactly. Missing, duplicate and unknown combinations are
  reported on `variants` with the combination named.
- For each variant, `variants[i].price` is greater than 0 and at most 9,007,199,254,740,991.
  `variants[i].stock` is 0 or more, and `variants[i].weightGrams` is greater than 0.

## Merchant Centre images (`/merchant/products/{id}/images`)

| Method and route | Request | Success | Errors |
|------------------|---------|---------|--------|
| `POST /merchant/products/{id}/images` | `multipart/form-data`, field `file` | 201 `MerchantProductResponse` (image appended) | 400 (`file`: missing, over 5 MB, or not JPEG, PNG or WebP by content), 401, 403, 404, 409 `image-limit-reached`, `product-removed` or `product-version-conflict` (concurrent change; retry) |
| `PUT /merchant/products/{id}/images/order` | `{ "imageIds": [ … ], "version": n }` | 200 `MerchantProductResponse` | 400 (not a permutation of the current images), 401, 403, 404, 409 |
| `DELETE /merchant/products/{id}/images/{imageId}?version=n` | | 200 `MerchantProductResponse` | 401, 403, 404 (Product or image), 409 |
| `GET /merchant/products/{id}/images/{imageId}` | | 200 image bytes, `Content-Type` from the sniffed format, `Cache-Control: private, no-cache`, `ETag` (the image ID), `X-Content-Type-Options: nosniff` | 401, 403, 404 |

The request body limit for upload is 6 MB. The 5 MB limit applies to the file itself (research R7).

The Merchant Centre and `/ops` image routes require a bearer token. The web app fetches them with
`fetch` and shows them through object URLs (`lib/authImage.ts`, research R10). Only Marketplace image
URLs can be used directly in `<img src>`.

---

## Marketplace (`/marketplace`, public)

| Method and route | Success | Errors |
|------------------|---------|--------|
| `GET /marketplace/categories` | 200 `CategoryNode[]`. The whole tree, siblings sorted by name ignoring case | |
| `GET /marketplace/products?query&categoryId&shopId&page&pageSize` | 200 `Page<ProductCard>` | 400 for `page` below 1, `pageSize` below 1, or `query` over 200 characters |
| `GET /marketplace/products/{id}` | 200 `ProductDetail` | 404 if the Product is unknown or not Published |
| `GET /marketplace/products/{id}/images/{imageId}` | 200 image bytes, `Cache-Control: no-cache`, `ETag` (the image ID; 304 on `If-None-Match`), `X-Content-Type-Options: nosniff` | 404 if the Product is not Published or the image is unknown |
| `GET /marketplace/shops/{id}` | 200 `ShopPage` | 404 if the Shop is unknown |

**Paging**: `page` defaults to 1 and `pageSize` to 20. A `pageSize` above 100 is clamped to 100. A
page past the end returns `items: []` with the real `totalCount`.

**Query semantics**:

- `query` uses web-search syntax with English stemming, and every word must match.
- With a query, results are ordered by relevance, then newest first. Without one, newest first.
- `categoryId` includes all descendants. An unknown `categoryId` returns an empty page.
- `shopId` limits results to one Merchant's Products. The Shop page uses it.

```jsonc
// Page<T>
{ "items": [ … ], "page": 1, "pageSize": 20, "totalCount": 137 }

// CategoryNode
{ "id": "…", "name": "Home", "children": [ { "id": "…", "name": "Kitchen", "children": [] } ] }

// ProductCard: no stock, no weight, no Merchant internals
{ "id": "…", "name": "…", "categoryId": "…", "minPrice": 120000, "maxPrice": 150000,
  "available": true, "coverImageUrl": "/api/marketplace/products/{id}/images/{imageId}" /* or null */,
  "shop": { "id": "…", "shopName": "Sunny Mugs" } }

// ProductDetail
{ "id": "…", "name": "…", "description": "…",
  "category": { "id": "…", "name": "Mugs" },
  "options": [ { "name": "Colour", "values": ["Red","Blue"] } ],
  "variants": [ { "id": "…", "optionValues": ["Red"], "price": 120000, "availability": 10 } ],
  "images": [ { "id": "…", "url": "…" } ],     // in position order; the first is the cover
  "shop": { "id": "…", "shopName": "Sunny Mugs" } }

// ShopPage: no Pickup Address (FR-031)
{ "id": "…", "shopName": "Sunny Mugs", "description": "…" }
```

---

## Platform operations (`/ops`)

| Method and route | Request | Success | Errors |
|------------------|---------|---------|--------|
| `POST /ops/categories` | `{ "name": "…", "parentId": "…" /* or null */ }` | 201 `CategoryResponse`, `Location` | 400 (name, or an unknown `parentId` reported on `parentId`), 401, 403, 409 `category-has-products` or `category-name-taken` |
| `GET /ops/categories/{id}` | | 200 `CategoryResponse` | 401, 403, 404 |
| `PUT /ops/categories/{id}` | `{ "name": "…" }` | 200 `CategoryResponse` | 400, 401, 403, 404, 409 `category-name-taken` |
| `DELETE /ops/categories/{id}` | | 204 | 401, 403, 404, 409 `category-has-children` or `category-has-products` |
| `GET /ops/products/{id}` | | 200 `OperatorProductResponse` | 401, 403, 404 |
| `POST /ops/products/{id}/remove` | | 200 `OperatorProductResponse` (idempotent) | 401, 403, 404 |
| `GET /ops/products/{id}/images/{imageId}` | | 200 image bytes (headers as for the Merchant route) | 401, 403, 404 |

```jsonc
// CategoryResponse
{ "id": "…", "name": "…", "parentId": "…" /* or null */ }

// OperatorProductResponse = MerchantProductResponse
//   + "merchantId" + "shopName"
//   with image URLs under /api/ops/products/{id}/images/{imageId}
```

A Platform Operator's removal takes no `version`. It loads, removes and saves, and retries once on a
concurrency conflict, so a moderation action is never lost to a Merchant's concurrent edit
(research R5).
