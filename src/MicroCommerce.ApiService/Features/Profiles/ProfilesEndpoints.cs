using System.Security.Claims;
using MediatR;
using MicroCommerce.ApiService.Features.Profiles.Application.Commands.AddAddress;
using MicroCommerce.ApiService.Features.Profiles.Application.Commands.DeleteAddress;
using MicroCommerce.ApiService.Features.Profiles.Application.Commands.RemoveAvatar;
using MicroCommerce.ApiService.Features.Profiles.Application.Commands.SetDefaultAddress;
using MicroCommerce.ApiService.Features.Profiles.Application.Commands.UpdateAddress;
using MicroCommerce.ApiService.Features.Profiles.Application.Commands.UpdateProfile;
using MicroCommerce.ApiService.Features.Profiles.Application.Commands.UploadAvatar;
using MicroCommerce.ApiService.Features.Profiles.Application.Queries.GetProfile;

namespace MicroCommerce.ApiService.Features.Profiles;

/// <summary>
/// Profiles module endpoints.
/// Provides user profile management including display name, avatar, and addresses.
/// </summary>
public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/profiles")
            .WithTags("Profiles")
            .RequireAuthorization();

        group.MapGet("/me", GetMyProfile)
            .WithName("GetMyProfile")
            .WithSummary("Get or auto-create profile for current user")
            .Produces<ProfileDto>();

        group.MapPut("/me", UpdateProfile)
            .WithName("UpdateProfile")
            .WithSummary("Update profile display name")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapPost("/me/avatar", UploadAvatar)
            .WithName("UploadAvatar")
            .WithSummary("Upload profile avatar (max 5MB)")
            .DisableAntiforgery()
            .Produces<UploadAvatarResult>();

        group.MapDelete("/me/avatar", RemoveAvatar)
            .WithName("RemoveAvatar")
            .WithSummary("Remove profile avatar")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/me/addresses", AddAddress)
            .WithName("AddAddress")
            .WithSummary("Add new address to profile")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/me/addresses/{addressId:guid}", UpdateAddress)
            .WithName("UpdateAddress")
            .WithSummary("Update existing address")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/me/addresses/{addressId:guid}", DeleteAddress)
            .WithName("DeleteAddress")
            .WithSummary("Delete address from profile")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/me/addresses/{addressId:guid}/default", SetDefaultAddress)
            .WithName("SetDefaultAddress")
            .WithSummary("Set address as default")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> GetMyProfile(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);
        var profile = await sender.Send(new GetProfileQuery(userId), cancellationToken);
        return profile is null ? Results.Problem("Failed to create profile") : Results.Ok(profile);
    }

    private static async Task<IResult> UpdateProfile(
        UpdateProfileRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);
        await sender.Send(new UpdateProfileCommand(userId, request.DisplayName), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UploadAvatar(
        IFormFile file,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest(new { detail = "No file provided" });

        if (!file.ContentType.StartsWith("image/"))
            return Results.BadRequest(new { detail = "File must be an image" });

        if (file.Length > 5 * 1024 * 1024)
            return Results.BadRequest(new { detail = "Image must be less than 5MB" });

        var userId = GetUserId(httpContext);
        using var stream = file.OpenReadStream();
        var avatarUrl = await sender.Send(
            new UploadAvatarCommand(userId, stream, file.FileName),
            cancellationToken);

        return Results.Ok(new UploadAvatarResult(avatarUrl));
    }

    private static async Task<IResult> RemoveAvatar(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);
        await sender.Send(new RemoveAvatarCommand(userId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AddAddress(
        AddAddressRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var addressId = await sender.Send(new AddAddressCommand(
            userId,
            request.Name,
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country,
            request.SetAsDefault), cancellationToken);

        return Results.Created($"/api/profiles/me/addresses/{addressId}", new { id = addressId });
    }

    private static async Task<IResult> UpdateAddress(
        Guid addressId,
        UpdateAddressRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        await sender.Send(new UpdateAddressCommand(
            userId,
            addressId,
            request.Name,
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country), cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAddress(
        Guid addressId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);
        await sender.Send(new DeleteAddressCommand(userId, addressId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> SetDefaultAddress(
        Guid addressId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);
        await sender.Send(new SetDefaultAddressCommand(userId, addressId), cancellationToken);
        return Results.NoContent();
    }

    private static Guid GetUserId(HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }
}

// Request records for endpoint contracts
public sealed record UpdateProfileRequest(string DisplayName);

public sealed record AddAddressRequest(
    string Name,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    bool SetAsDefault = false);

public sealed record UpdateAddressRequest(
    string Name,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);

public sealed record UploadAvatarResult(string AvatarUrl);
