using Azure.Storage;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Photos;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using MicroCommerce.Catalog.Infrastructure.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicroCommerce.Catalog.Infrastructure;

public static class InfrastructureExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // Wire IEntityTypeConfiguration<T> implementations from this assembly into AppDbContext.
        // AppDbContext lives in Application and must not reference Infrastructure, so this static
        // bridge lets Infrastructure own model configuration without inverting the layer arrow.
        AppDbContext.ConfigureModel = modelBuilder =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);

        builder.AddNpgsqlDbContext<AppDbContext>("catalogdb",
            configureDbContextOptions: o =>
            {
                o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                // Migrations live alongside the EF mappings in the Infrastructure assembly.
                o.UseNpgsql(npg => npg.MigrationsAssembly(typeof(ProductConfiguration).Assembly.FullName));
            });

        var photosConnection = builder.Configuration.GetConnectionString("photos");
        if (!string.IsNullOrWhiteSpace(photosConnection))
            builder.Services.AddSingleton(ParsePhotosSharedKey(photosConnection));
        builder.Services.AddSingleton<IPhotoUploadUrlIssuer, BlobPhotoUploadUrlIssuer>();
        builder.Services.AddSingleton<IClock, DemoClock>();
        return builder;
    }

    // Azure/Azurite connection strings carry AccountName + AccountKey; extract them so the photo SAS
    // issuer can sign upload URIs with the shared key (see PhotoUploadSas).
    private static StorageSharedKeyCredential ParsePhotosSharedKey(string connectionString)
    {
        string? account = null, key = null;
        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');
            if (separator < 0) continue;
            var name = part[..separator];
            var value = part[(separator + 1)..];
            if (name.Equals("AccountName", StringComparison.OrdinalIgnoreCase)) account = value;
            else if (name.Equals("AccountKey", StringComparison.OrdinalIgnoreCase)) key = value;
        }
        return new StorageSharedKeyCredential(
            account ?? throw new InvalidOperationException("photos connection string missing AccountName"),
            key ?? throw new InvalidOperationException("photos connection string missing AccountKey"));
    }
}
