using MediatR;
using MicroCommerce.ApiService.Features.Cart;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.SimulatePayment;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;
using MicroCommerce.ApiService.Features.Ordering.Application.Commands.UpdateOrderStatus;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetAllOrders;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderById;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrderDashboard;
using MicroCommerce.ApiService.Features.Ordering.Application.Queries.GetOrdersByBuyer;

namespace MicroCommerce.ApiService.Features.Ordering;

/// <summary>
/// Ordering module endpoints.
/// Provides checkout, payment simulation, order retrieval, order history, and admin management.
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
            .Produces<Guid>(StatusCodes.Status201Created)
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

        group.MapGet("/orders/my", GetMyOrders)
            .WithName("GetMyOrders")
            .WithSummary("Get orders for the current buyer (paginated)")
            .Produces<OrderListDto>();

        group.MapGet("/orders", GetAllOrders)
            .WithName("GetAllOrders")
            .WithSummary("Get all orders (admin, paginated)")
            .Produces<OrderListDto>();

        group.MapGet("/dashboard", GetDashboard)
            .WithName("GetOrderDashboard")
            .WithSummary("Get order dashboard statistics (admin)")
            .Produces<OrderDashboardDto>();

        group.MapPatch("/orders/{id:guid}/status", UpdateOrderStatus)
            .WithName("UpdateOrderStatus")
            .WithSummary("Update order status (admin: Ship or Deliver)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

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

        Guid orderId = await sender.Send(command, cancellationToken);

        return Results.Created($"/api/ordering/orders/{orderId}", orderId);
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

    private static async Task<IResult> GetMyOrders(
        HttpContext httpContext,
        ISender sender,
        string? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        Guid buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);
        GetOrdersByBuyerQuery query = new(buyerId, status, page, pageSize);
        OrderListDto result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAllOrders(
        ISender sender,
        string? status = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        GetAllOrdersQuery query = new(status, page, pageSize);
        OrderListDto result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetDashboard(
        ISender sender,
        string timeRange = "today",
        CancellationToken cancellationToken = default)
    {
        GetOrderDashboardQuery query = new(timeRange);
        OrderDashboardDto result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateOrderStatus(
        Guid id,
        UpdateOrderStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        UpdateOrderStatusCommand command = new(id, request.NewStatus);
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }
}

// Request records for endpoint contracts
public sealed record CheckoutRequest(
    string Email,
    ShippingAddressRequest ShippingAddress,
    List<OrderItemRequest> Items);

public sealed record PaymentRequest(bool ShouldSucceed);

public sealed record UpdateOrderStatusRequest(string NewStatus);
