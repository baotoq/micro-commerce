using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MicroCommerce.Catalog.Infrastructure.Photos;

namespace MicroCommerce.Catalog.FunctionalTests.Photos;

/// <summary>
/// Pure unit tests (no Azurite/Docker) for product-photo SAS generation against a CUSTOM-host
/// path-style blob endpoint, as produced by the Kubernetes deployment
/// (BlobEndpoint=http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1).
/// </summary>
public class PhotoUploadSasTests
{
    // Azurite well-known dev shared key (public, dev-only).
    private const string DevKey =
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    private static readonly Uri CustomHostBlob =
        new("http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1/photos/products/abc123.png");

    private static BlobSasBuilder NewBuilder()
    {
        var builder = new BlobSasBuilder
        {
            BlobContainerName = "photos",
            BlobName = "products/abc123.png",
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15),
            ContentType = "image/png",
        };
        builder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);
        return builder;
    }

    [Fact]
    public void GenerateSasUri_against_custom_host_path_style_endpoint_throws()
    {
        // Documents the root cause: the SDK treats a non-IP host as subdomain-style and mis-parses
        // the account segment ("devstoreaccount1") in the path as the container, so GenerateSasUri
        // rejects the explicit "photos" container as a mismatch.
        var credential = new StorageSharedKeyCredential("devstoreaccount1", DevKey);
        var blob = new BlobClient(CustomHostBlob, credential);

        Assert.Throws<InvalidOperationException>(() => blob.GenerateSasUri(NewBuilder()));
    }

    [Fact]
    public void BuildUploadUri_produces_valid_sas_for_custom_host_path_style_endpoint()
    {
        var credential = new StorageSharedKeyCredential("devstoreaccount1", DevKey);

        var uri = PhotoUploadSas.BuildUploadUri(CustomHostBlob, NewBuilder(), credential);

        // Path is preserved verbatim (account/container/blob), and a signed SAS is appended.
        Assert.Equal("/devstoreaccount1/photos/products/abc123.png", uri.AbsolutePath);
        Assert.Contains("sig=", uri.Query);
        Assert.Contains("sp=", uri.Query);
        Assert.Contains("se=", uri.Query);
    }
}
