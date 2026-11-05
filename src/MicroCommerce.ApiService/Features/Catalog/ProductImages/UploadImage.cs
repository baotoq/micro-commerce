using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog.MerchantProducts;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.ProductImages;

public sealed class UploadImageRequest
{
    public Guid Id { get; set; }

    public IFormFile? File { get; set; }
}

public sealed class UploadImageValidator : Validator<UploadImageRequest>
{
    public UploadImageValidator()
    {
        RuleFor(x => x.File).NotNull().WithMessage("file is required.");
        RuleFor(x => x.File!.Length).InclusiveBetween(1, ProductImage.MaxSizeBytes)
            .When(x => x.File is not null)
            .WithMessage("file must be at most 5 MB.")
            .OverridePropertyName("File");
    }
}

/// <summary>
/// A Merchant adds an image to its Product. The content is judged before anything is attached; the
/// blob is written first and deleted again if the Product can't be saved (FR-020, FR-022, research R7).
/// </summary>
public sealed class UploadImageEndpoint(
    MerchantCatalog catalog,
    IProductImageStore store,
    TimeProvider timeProvider,
    ILogger<UploadImageEndpoint> logger) : Endpoint<UploadImageRequest, MerchantProductResponse>
{
    public override void Configure()
    {
        Post("/products/{id}/images");
        Group<MerchantCentreGroup>();
        AllowFileUploads();
        // The image may be 5 MB; the rest is room for the multipart envelope.
        MaxRequestBodySize(6 * 1024 * 1024);
        Summary(s =>
        {
            s.Summary = "Upload a Product image (JPEG, PNG or WebP, at most 5 MB)";
            s.Responses[201] = "The image was added at the end.";
            s.Responses[400] = "file is missing, over 5 MB, or not a JPEG, PNG or WebP image by its content.";
            s.Responses[404] = "No such Product among the Merchant's own.";
            s.Responses[409] = "image-limit-reached, product-removed, or product-version-conflict (a concurrent change; retry).";
        });
    }

    public override async Task HandleAsync(UploadImageRequest req, CancellationToken ct)
    {
        var file = req.File!;
        var head = new byte[ImageFormats.HeaderLength];
        int read;
        await using (var peek = file.OpenReadStream())
        {
            read = await peek.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false, ct);
        }

        if (ImageFormats.Detect(head.AsSpan(0, read)) is not { } format)
        {
            AddError(r => r.File!, "file must be a JPEG, PNG or WebP image.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var product = await this.LoadForChangeAsync(catalog, req.Id, version: null, ct);
        if (product is null) return;
        if (product.Images.Count >= Product.MaxImages)
        {
            await Send.ResultAsync(Problems.Conflict(Problems.ImageLimitReached));
            return;
        }

        var imageId = ProductImageId.New();
        await using (var content = file.OpenReadStream())
        {
            await store.PutAsync(product.Id, imageId, content, format.ContentType(), ct);
        }

        try
        {
            product.AddImage(imageId, format, file.Length, timeProvider.GetUtcNow());
            await catalog.Db.SaveChangesAsync(ct);
        }
        catch (Exception e) when (e is ImageLimitReachedException or DbUpdateConcurrencyException)
        {
            // Never leave an unreferenced blob behind if we can help it (FR-022).
            if (!await store.TryDeleteAsync(product.Id, imageId, CancellationToken.None))
                logger.LogWarning("Orphaned image blob {ProductId}/{ImageId} could not be deleted", product.Id, imageId);
            await Send.ResultAsync(Problems.Conflict(e is ImageLimitReachedException ? Problems.ImageLimitReached : Problems.ProductVersionConflict));
            return;
        }

        await Send.ResponseAsync(MerchantProductResponse.From(product), StatusCodes.Status201Created, ct);
    }
}
