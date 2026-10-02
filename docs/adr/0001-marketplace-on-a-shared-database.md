# Marketplace on a shared database, scoped by Merchant

MicroCommerce is a Shopee-style marketplace, not a Shopify-style storefront builder: Buyers search one shared catalog and check out across several Merchants at once. Because cross-Merchant search and Checkout are core, all Merchants share one database and every Merchant-owned row carries a `MerchantId`; the Merchant-facing API scopes reads and writes to the signed-in Merchant automatically. Ownership is attached to the Merchant, never to an Account, so Merchant staff can be added later without re-keying data.

## Considered Options

- **Schema or database per Merchant**: rejected. It isolates Merchants strongly, but every Buyer-facing query (search, Checkout spanning Merchants) would have to fan out across stores.
- **Storefront builder (one store per Merchant, Buyers scoped per store)**: rejected. It doesn't fit the "Buyers shop across Merchants" model we want.
