using Vogen;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Reviews.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Wishlists.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Common.Persistence;

[EfCoreConverter<ProductId>]
[EfCoreConverter<CategoryId>]
[EfCoreConverter<OrderId>]
[EfCoreConverter<OrderItemId>]
[EfCoreConverter<CartId>]
[EfCoreConverter<CartItemId>]
[EfCoreConverter<StockItemId>]
[EfCoreConverter<ReservationId>]
[EfCoreConverter<AdjustmentId>]
[EfCoreConverter<UserProfileId>]
[EfCoreConverter<AddressId>]
[EfCoreConverter<ReviewId>]
[EfCoreConverter<WishlistItemId>]
[EfCoreConverter<CouponId>]
[EfCoreConverter<CouponUsageId>]
internal sealed partial class VogenEfCoreConverters;
