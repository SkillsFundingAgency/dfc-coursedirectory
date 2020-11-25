using System;

namespace Dfc.Providerportal.FindAnApprenticeship.Storage
{
    public class AzureBlobStorageClientOptions
    {
        public string ConnectionString { get; }

        public string BlobContainerName { get; }

        public AzureBlobStorageClientOptions(string connectionString, string blobContainerName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(blobContainerName))
            {
                throw new ArgumentNullException(nameof(blobContainerName));
            }

            ConnectionString = connectionString;
            BlobContainerName = blobContainerName;
        }
    }
}