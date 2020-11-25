using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Dfc.Providerportal.FindAnApprenticeship.Storage
{
    public interface IBlobStorageClient
    {
        BlobClient GetBlobClient(string blobName);

        BlobLeaseClient GetBlobLeaseClient(BlobClient blobClient, string leaseId = null);
    }
}