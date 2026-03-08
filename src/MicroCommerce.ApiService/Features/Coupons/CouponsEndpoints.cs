using MediatR;
using MicroCommerce.ApiService.Features.Coupons.Application.Commands.CreateCoupon;
using MicroCommerce.ApiService.Features.Coupons.Application.Commands.ToggleCouponStatus;
using MicroCommerce.ApiService.Features.Coupons.Application.Commands.UpdateCoupon;
using MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCouponById;
using MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCoupons;
using MicroCommerce.ApiService.Features.Coupons.Application.Queries.ValidateCoupon;

namespace MicroCommerce.ApiService.Features.Coupons;

public static class CouponsEndpoints
{
    public static IEndpointRouteBuilder MapCouponsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/coupons")
            .WithTags("Coupons");

        // Admin endpoints
        group.MapPost("/", CreateCoupon)
            .WithName("CreateCoupon")
            .WithSummary("Create a new coupon")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", GetCoupons)
            .WithName("GetCoupons")
            .WithSummary("Get paginated list of coupons")
            .Produces<CouponListDto>();

        group.MapGet("/{id:guid}", GetCouponById)
            .WithName("GetCouponById")
            .WithSummary("Get a coupon by ID")
            .Produces<CouponDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateCoupon)
            .WithName("UpdateCoupon")
            .WithSummary("Update a coupon")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", DeleteCoupon)
            .WithName("DeleteCoupon")
            .WithSummary("Soft-delete a coupon")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", ToggleCouponStatus)
            .WithName("ToggleCouponStatus")
            .WithSummary("Activate or deactivate a coupon")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Customer-facing endpoint
        group.MapPost("/validate", ValidateCoupon)
            .WithName("ValidateCoupon")
            .WithSummary("Validate a coupon code and calculate discount")
            .Produces<ValidateCouponResult>();

        return endpoints;
    }

    private static async Task<IResult> CreateCoupon(
        CreateCouponRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        CreateCouponCommand command = new(
            request.Code,
            request.Description,
            request.DiscountType,
            request.DiscountValue,
            request.ValidFrom,
            request.ValidUntil,
            request.MinOrderAmount,
            request.MaxDiscountAmount,
            request.UsageLimit,
            request.UsagePerUser,
            request.ApplicableProductIds,
            request.ApplicableCategoryIds);

        Guid id = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/coupons/{id}", id);
    }

    private static async Task<IResult> GetCoupons(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        bool? isActive = null,
        string? search = null)
    {
        CouponListDto result = await sender.Send(
            new GetCouponsQuery(page, pageSize, isActive, search),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetCouponById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        CouponDto? coupon = await sender.Send(new GetCouponByIdQuery(id), cancellationToken);
        return coupon is null ? Results.NotFound() : Results.Ok(coupon);
    }

    private static async Task<IResult> UpdateCoupon(
        Guid id,
        UpdateCouponRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        UpdateCouponCommand command = new(
            id,
            request.Description,
            request.DiscountType,
            request.DiscountValue,
            request.ValidFrom,
            request.ValidUntil,
            request.MinOrderAmount,
            request.MaxDiscountAmount,
            request.UsageLimit,
            request.UsagePerUser,
            request.ApplicableProductIds,
            request.ApplicableCategoryIds);

        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCoupon(
        Guid id,
        ISender sender,
        Infrastructure.CouponsDbContext context,
        CancellationToken cancellationToken)
    {
        Domain.ValueObjects.CouponId couponId = Domain.ValueObjects.CouponId.From(id);
        Domain.Entities.Coupon? coupon = await context.Coupons
            .FindAsync([couponId], cancellationToken);

        if (coupon is null)
            return Results.NotFound();

        context.Coupons.Remove(coupon);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ToggleCouponStatus(
        Guid id,
        ToggleCouponStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ToggleCouponStatusCommand(id, request.IsActive), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ValidateCoupon(
        ValidateCouponRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        ValidateCouponResult result = await sender.Send(
            new ValidateCouponQuery(request.Code, request.Subtotal, request.UserId),
            cancellationToken);

        return Results.Ok(result);
    }
}

public sealed record CreateCouponRequest(
    string Code,
    string Description,
    string DiscountType,
    decimal DiscountValue,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil = null,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int? UsageLimit = null,
    int? UsagePerUser = null,
    List<Guid>? ApplicableProductIds = null,
    List<Guid>? ApplicableCategoryIds = null);

public sealed record UpdateCouponRequest(
    string Description,
    string DiscountType,
    decimal DiscountValue,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil = null,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int? UsageLimit = null,
    int? UsagePerUser = null,
    List<Guid>? ApplicableProductIds = null,
    List<Guid>? ApplicableCategoryIds = null);

public sealed record ToggleCouponStatusRequest(bool IsActive);

public sealed record ValidateCouponRequest(string Code, decimal Subtotal, string? UserId = null);
