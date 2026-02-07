using MediatR;
using MicroCommerce.ApiService.Features.Cart;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.SimulatePayment;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderById;

namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>
/// Ordering module endpoints.
/// Provides checkout, payment simulation, and order retrieval.
/// </summary>
public static class OrderingEndpoints
{
    public static IEndpointRouteBuilder MapOrderingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/ordering")
            .WithTags("Ordering");

        group.MapPost("/checkout", Checkout)
            .WithName("Checkout")
            .WithSummary("Submit a new order from cart items")
            .Produces<SubmitOrderResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/orders/{id:guid}/pay", SimulatePayment)
            .WithName("SimulatePayment")
            .WithSummary("Simulate payment for an order (dev/test)")
            .Produces<SimulatePaymentResult>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/orders/{id:guid}", GetOrderById)
            .WithName("GetOrderById")
            .WithSummary("Get order details by ID")
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> Checkout(
        CheckoutRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Guid buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);

        SubmitOrderCommand command = new(
            buyerId,
            request.Email,
            request.ShippingAddress,
            request.Items);

        SubmitOrderResult result = await sender.Send(command, cancellationToken);

        return Results.Created($"/api/ordering/orders/{result.OrderId}", result);
    }

    private static async Task<IResult> SimulatePayment(
        Guid id,
        PaymentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        SimulatePaymentCommand command = new(id, request.ShouldSucceed);
        SimulatePaymentResult result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetOrderById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        OrderDto? result = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}

// Request records for endpoint contracts
public sealed record CheckoutRequest(
    string Email,
    ShippingAddressRequest ShippingAddress,
    List<OrderItemRequest> Items);

public sealed record PaymentRequest(bool ShouldSucceed);
