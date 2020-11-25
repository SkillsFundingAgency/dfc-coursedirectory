using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Dfc.Providerportal.FindAnApprenticeship.Storage
{
    public class AzureBlobStorageClient : IBlobStorageClient
    {
        private readonly BlobContainerClient _blobContainerClient;

        public AzureBlobStorageClient(AzureBlobStorageClientOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _blobContainerClient = new BlobContainerClient(options.ConnectionString, options.BlobContainerName);
            _blobContainerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public BlobClient GetBlobClient(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            return _blobContainerClient.GetBlobClient(blobName);
        }

        public BlobLeaseClient GetBlobLeaseClient(BlobClient blobClient, string leaseId = null)
        {
            if (blobClient == null)
            {
                throw new ArgumentNullException(nameof(blobClient));
            }

            return blobClient.GetBlobLeaseClient(leaseId);
        }
    }
}