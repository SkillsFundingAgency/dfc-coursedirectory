using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;

namespace Dfc.CourseDirectory.WebV2.BinaryStorageProvider
{
    public class BlobStorageBinaryStorageProvider : IBinaryStorageProvider
    {
        private readonly CloudBlobClient _blobClient;
        private readonly string _containerName;

        public BlobStorageBinaryStorageProvider(
            string accountName,
            string accountKey,
            string containerName)
        {
            var account = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), useHttps: true);
            _blobClient = account.CreateCloudBlobClient();

            _containerName = containerName;
        }

        public async Task<bool> TryDownloadFile(string path, Stream destination)
        {
            var container = await GetContainer();
            var blobRef = container.GetBlockBlobReference(path);

            if (!await blobRef.ExistsAsync())
            {
                return false;
            }

            await blobRef.DownloadToStreamAsync(destination);
            return true;
        }

        public async Task UploadFile(string path, Stream source)
        {
            var container = await GetContainer();
            var blobRef = container.GetBlockBlobReference(path);

            await blobRef.UploadFromStreamAsync(source);
        }

        private async Task<CloudBlobContainer> GetContainer()
        {
            var container = _blobClient.GetContainerReference(_containerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }
    }
}
