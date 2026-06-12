using System.Security.Claims;
using MediatR;
using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Queries;

namespace MicroCommerce.Catalog.Api.Endpoints;

/// <summary>Buyer-facing order endpoints (storefront checkout + confirmation read).</summary>
public static class StorefrontOrderEndpoints
{
    /// <summary>Checkout body — everything except CustomerEmail, which comes from the token.</summary>
    public record StorefrontCheckoutRequest(
        string CustomerName,
        string ShipLine1,
        string ShipLine2,
        string CityState,
        string ShippingMethod,
        decimal ShippingPaid,
        decimal Tax,
        string PaymentBrand,
        string PaymentLastFour,
        string? PromoCode,
        IReadOnlyList<StorefrontLineInput> Lines);

    public static IEndpointRouteBuilder MapStorefrontOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Storefront")
            .RequireAuthorization(AuthenticationExtensions.BuyerPolicy);

        group.MapPost("/checkout", async (
            StorefrontCheckoutRequest request, ClaimsPrincipal user, ISender mediator, CancellationToken ct) =>
        {
            var email = TokenEmail(user);
            if (email is null) return Results.Unauthorized();

            var result = await mediator.Send(new PlaceStorefrontOrderCommand(
                email,
                request.CustomerName,
                request.ShipLine1,
                request.ShipLine2,
                request.CityState,
                request.ShippingMethod,
                request.ShippingPaid,
                request.Tax,
                request.PaymentBrand,
                request.PaymentLastFour,
                request.PromoCode,
                request.Lines), ct);

            if (result.IsFailure)
            {
                return Results.Problem(
                    title: result.Error!.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
            var created = result.Value!;
            return Results.Created($"/api/orders/{created.Number}/confirmation", created);
        })
        .WithName("StorefrontCheckout");

        // Buyer-scoped read: only the order's own customer may fetch it (email claim match).
        group.MapGet("/{number:int}/confirmation", async (
            int number, ClaimsPrincipal user, ISender mediator, CancellationToken ct) =>
        {
            var email = TokenEmail(user);
            if (email is null) return Results.Unauthorized();

            var order = await mediator.Send(new GetOrderByNumberQuery(number), ct);
            if (order is null || !string.Equals(order.Customer.Email, email, StringComparison.OrdinalIgnoreCase))
                return Results.NotFound();
            return Results.Ok(order);
        })
        .WithName("StorefrontOrderConfirmation");

        return app;
    }

    // MapInboundClaims=false keeps original claim names: prefer "email", fall back to
    // "preferred_username" (usernames are emails in this realm; see realm import).
    private static string? TokenEmail(ClaimsPrincipal user) =>
        user.FindFirst("email")?.Value ?? user.FindFirst("preferred_username")?.Value;
}
