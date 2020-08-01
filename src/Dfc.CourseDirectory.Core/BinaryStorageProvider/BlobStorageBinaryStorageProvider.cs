using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs;
using Azure;

namespace Dfc.CourseDirectory.Core.BinaryStorageProvider
{
    public class BlobStorageBinaryStorageProvider : IBinaryStorageProvider
    {
        private readonly BlobContainerClient _blobContainerClient;
        private bool _containerExists;

        public BlobStorageBinaryStorageProvider(IOptions<BlobStorageBinaryStorageProviderSettings> options)
        {
            _blobContainerClient = new BlobContainerClient(options.Value.ConnectionString, options.Value.ContainerName);
        }

        public async Task<bool> TryDownloadFile(string path, Stream destination)
        {
            await EnsureContainerExists();

            var blob = _blobContainerClient.GetBlobClient(path);

            try
            {
                await blob.DownloadToAsync(destination);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        public async Task UploadFile(string path, Stream source)
        {
            await EnsureContainerExists();

            var blob = _blobContainerClient.GetBlobClient(path);
            await blob.UploadAsync(source);
        }

        private async Task EnsureContainerExists()
        {
            if (!_containerExists)
            {
                await _blobContainerClient.CreateIfNotExistsAsync();
                _containerExists = true;
            }
        }
    }
}
