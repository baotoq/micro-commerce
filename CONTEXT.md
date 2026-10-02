# MicroCommerce

A multi-merchant marketplace in the style of Shopee: many Merchants sell through one shared catalog, and Buyers shop across all of them in a single place.

## Language

### Parties

**Marketplace**:
The single shared storefront and catalog through which every Merchant sells and every Buyer shops.
_Avoid_: Platform (when meaning the storefront), mall, store

**Account**:
The identity a person signs in with. Every Account can act as a Buyer; an Account may also own one Merchant.
_Avoid_: User, login, profile

**Merchant**:
A selling business on the Marketplace, owned by exactly one Account. Products, Orders and money belong to the Merchant, not to the owning Account. A Merchant has a Pickup Address its parcels ship from.
_Avoid_: Seller, vendor, tenant

**Suspended Merchant**:
A Merchant a Platform Operator has barred from selling: its Products are hidden and it cannot receive new Orders, but its in-flight Orders continue normally.
_Avoid_: Banned, disabled, deactivated

**Shop**:
A Merchant's public page on the Marketplace. Each Merchant has exactly one Shop; it is a view of the Merchant, not a separate thing.
_Avoid_: Store, storefront

**Buyer**:
The role an Account plays when purchasing on the Marketplace. An Account cannot buy from its own Merchant.
_Avoid_: Customer, shopper, user

**Platform Operator**:
Staff running the Marketplace who can suspend Merchants and remove listings. Merchants do not need their approval to start selling.
_Avoid_: Admin, moderator

**Merchant Centre**:
The area of the Marketplace where a Merchant manages its Shop, Products, Orders and Merchant Balance.
_Avoid_: Seller Centre, dashboard, back office

### Catalog

**Product**:
A listing a Merchant offers, holding its name, description, images and Category. A Product is never bought directly; its Variants are.
_Avoid_: Item, listing (as a separate concept)

**Product status**:
_Draft_ → _Published_; a Platform Operator can mark any Product _Removed_. Buyers only see Published Products of Merchants that are not suspended.

**Variant**:
One purchasable combination of a Product's options (e.g. Red / M), with its own price, stock and weight. A Product without options has a single default Variant.
_Avoid_: SKU, option, model

**Category**:
A node in the Marketplace-wide category tree maintained by Platform Operators. Every Product sits in exactly one leaf Category.
_Avoid_: Tag, collection, department

### Buying

**Checkout**:
A Buyer's single act of purchasing a cart that may span several Merchants, to one Delivery Address. One Checkout produces one Payment and one Order per Merchant.
_Avoid_: Cart order, master order, transaction

**Order**:
The part of a Checkout belonging to exactly one Merchant, shipped, tracked and cancelled independently of the other Orders.
_Avoid_: Sub-order, purchase, shipment

**Payment**:
The money a Buyer pays for one Checkout, collected once across all of its Orders.
_Avoid_: Charge, transaction

**Payment Attempt**:
One try at collecting a Checkout's Payment. A Buyer may retry after a failed attempt until the payment window closes; at most one attempt succeeds.
_Avoid_: Retry, transaction

**Reservation**:
Stock of a Variant held for a placed Checkout during its payment window. Released if the Checkout expires or the Order is cancelled before shipping; a cart never holds stock.
_Avoid_: Hold, lock, allocation

**Return Request**:
A Buyer's claim for a full Refund of a Delivered Order while its money is still in Escrow. It always covers the whole Order. The Merchant can accept or refuse it; silence past the Merchant response deadline counts as acceptance, and a refusal goes to a Platform Operator to decide.
_Avoid_: Dispute, claim, RMA

**Delivery Address**:
Where a Checkout's Orders are delivered; chosen once per Checkout and shared by all its Orders.
_Avoid_: Shipping address

### Lifecycle

**Payment window**:
How long a placed Checkout may stay Awaiting Payment before it Expires.

**Ship-by deadline**:
How long a Merchant has to ship an Order before it is cancelled automatically.

**Confirmation period**:
How long after Delivered an Order stays in Escrow, open to a Return Request, before it Completes on its own. It stops while a Return Request is open.

**Merchant response deadline**:
How long a Merchant has to answer a Return Request before it is accepted automatically.

**Checkout status**:
_Awaiting Payment_ → _Paid_, or _Expired_ if no Payment Attempt succeeds within the payment window.

**Order status**:
_To Ship_ → _Shipped_ → _Delivered_ → _Completed_; also _Cancelled_ (before shipping) and _Return Requested_ → _Refunded_ or back to the Delivered path if the request is rejected. An Order still To Ship can be cancelled by the Buyer or the Merchant, and is cancelled automatically when the ship-by deadline passes. An Order becomes Completed when the Buyer confirms receipt or the confirmation period lapses, and that is when its money leaves Escrow. While Return Requested, the Order stays in Escrow and does not Complete. Cancelled and Refunded Orders are paid back from Escrow, never from a Merchant Balance.

### Shipping

**Carrier**:
The logistics provider that quotes Shipping Fees from parcel weight and origin/destination provinces, and reports an Order's progress from Shipped to Delivered. One Order is one parcel.
_Avoid_: Courier, logistics partner

**Shipping Fee**:
The Carrier's price for delivering one Order, charged to the Buyer per Order and passed through the Marketplace to the Carrier. It never reaches a Merchant Balance, Commission is not taken from it, and it is refunded in full if the Order is cancelled before shipping.
_Avoid_: Delivery fee, postage

### Money

All amounts are in one Marketplace currency, and listed prices are final (tax is not modelled).

**Escrow**:
Money from a Payment that the Marketplace holds for an Order until the Buyer confirms receipt or the confirmation period lapses.
_Avoid_: Hold, pending funds

**Commission**:
The Marketplace's cut, a flat rate across the Marketplace, deducted from an Order's amount (excluding its Shipping Fee) when it leaves Escrow.
_Avoid_: Fee, take rate

**Merchant Balance**:
Money released from Escrow (minus Commission) that a Merchant has earned but not yet withdrawn.
_Avoid_: Wallet, earnings, account balance

**Refund**:
Money returned to a Buyer: for a Cancelled Order or accepted Return Request (taken from Escrow), or for a Payment that arrives after its Checkout Expired (returned in full; an Expired Checkout is never revived).
_Avoid_: Chargeback, reversal

**Payout**:
A Merchant's withdrawal of money from its Merchant Balance.
_Avoid_: Withdrawal, settlement, disbursement
