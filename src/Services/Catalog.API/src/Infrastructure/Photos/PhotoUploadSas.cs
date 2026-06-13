using Azure.Storage;
using Azure.Storage.Sas;

namespace MicroCommerce.Catalog.Infrastructure.Photos;

/// <summary>
/// Builds a write-only SAS upload URI by signing the blob with the shared key directly, instead of
/// <c>BlobClient.GenerateSasUri</c>.
///
/// <para>Why: <c>GenerateSasUri</c> validates that the builder's <c>BlobContainerName</c> matches the
/// container parsed from the client's URI. Against a path-style Azurite endpoint on a CUSTOM (non-IP)
/// host — e.g. <c>http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1/photos/...</c> — the SDK
/// assumes subdomain-style addressing and mis-parses the account segment (<c>devstoreaccount1</c>) as the
/// container, so it rejects the correct <c>photos</c> container with an InvalidOperationException.</para>
///
/// <para><c>ToSasQueryParameters</c> signs over the builder's container/blob names verbatim, which match
/// what Azurite (started with <c>--disableProductStyleUrl</c>) expects, so the resulting SAS is valid.
/// The blob URI itself is constructed by appending segments and is already correct.</para>
/// </summary>
public static class PhotoUploadSas
{
    public static Uri BuildUploadUri(Uri blobUri, BlobSasBuilder sas, StorageSharedKeyCredential credential)
    {
        var token = sas.ToSasQueryParameters(credential).ToString();
        return new UriBuilder(blobUri) { Query = token }.Uri;
    }

    /// <summary>
    /// Builds the blob URI by appending the container + blob to the service endpoint. Necessary because
    /// <c>BlobContainerClient.GetBlobClient(...).Uri</c> DROPS the container segment for custom-host
    /// path-style Azurite endpoints (the SDK assumes subdomain-style addressing), yielding e.g.
    /// <c>.../devstoreaccount1/products/x.png</c> instead of <c>.../devstoreaccount1/photos/products/x.png</c>.
    /// </summary>
    public static Uri BuildBlobUri(Uri serviceEndpoint, string containerName, string blobName)
        => new($"{serviceEndpoint.AbsoluteUri.TrimEnd('/')}/{containerName}/{blobName}");
}
