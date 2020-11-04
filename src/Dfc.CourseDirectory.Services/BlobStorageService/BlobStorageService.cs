using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Providers;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Services.BlobStorageService
{
    public class BlobFileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset? DateUploaded { get; set; }
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly ILogger<BlobStorageService> _log;

        private readonly string _accountName;
        private readonly string _accountKey;
        private readonly string _templatePath;
        private readonly string _apprenticeshipsTemplatePath;
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobContainer _container;
        private readonly int _inlineProcessingThreshold;

        public int InlineProcessingThreshold { get { return _inlineProcessingThreshold; } }

        public BlobStorageService(
            ILogger<BlobStorageService> logger,
            HttpClient httpClient,
            IOptions<BlobStorageSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _log = logger;
            _accountName = settings.Value.AccountName;
            _accountKey = settings.Value.AccountKey;
            _templatePath = settings.Value.TemplatePath;
            _apprenticeshipsTemplatePath = settings.Value.ApprenticeshipsTemplatePath;

            _account = new CloudStorageAccount(new StorageCredentials(_accountName, _accountKey), true);
            _container = _account.CreateCloudBlobClient().GetContainerReference(settings.Value.Container);

            _inlineProcessingThreshold = settings.Value.InlineProcessingThreshold;
            
            if (default(int) == _inlineProcessingThreshold)
            {
                _inlineProcessingThreshold = 10000; // if no setting then set the threshold really high so that all processing is inline
            }
        }

        public async Task DownloadFileAsync(string filePath, Stream stream)
        {
            try
            {
                var blockBlob = _container.GetBlockBlobReference(filePath);

                if (!await blockBlob.ExistsAsync())
                {
                    return;
                }

                _log.LogInformation($"Downloading {filePath} from blob storage");

                await blockBlob.DownloadToStreamAsync(stream);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Exception downloading {filePath}");
            }
        }

        public async Task UploadFileAsync(string filePath, Stream stream)
        {
            try
            {
                var blockBlob = _container.GetBlockBlobReference(filePath);
                
                var p = string.Join("/", filePath.Split("/").Reverse().Skip(1).Reverse());
                ArchiveFiles(p);

                stream.Position = 0;
                await blockBlob.UploadFromStreamAsync(stream);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Exception uploading {filePath}");
            }
        }

        public IEnumerable<BlobFileInfo> GetFileList(string filePath)
        {
            try
            {
                return _container
                    .GetDirectoryReference(filePath)?
                    .ListBlobs()?
                    .OfType<CloudBlockBlob>()?
                    .Select(b => new BlobFileInfo()
                    {
                        Name = b.Name,
                        Size = b.Properties.Length,
                        DateUploaded = b.Properties.Created 
                    });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Exception listing files at {filePath}");
                return null;
            }
        }

        public IEnumerable<CloudBlockBlob> ArchiveFiles(string filePath)
        {
            try
            {
                var blobs = _container?
                    .GetDirectoryReference(filePath)?
                    .ListBlobs()?
                    .OfType<CloudBlockBlob>();

                foreach (CloudBlockBlob b in blobs ?? Enumerable.Empty<CloudBlockBlob>())
                {
                    _container.GetBlobReference($"Archive/{filePath.Substring(0, 8)}_{b.Uri.Segments.Last()}").StartCopy(b.Uri);
                    b.DeleteIfExists();
                }

                return blobs;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Exception archiving files at {filePath}");
                return null;
            }
        }

        public Task GetBulkUploadTemplateFileAsync(Stream stream)
        {
            return GetBulkUploadTemplateFileAsync(stream, ProviderType.FE);
        }

        public Task GetBulkUploadTemplateFileAsync(Stream stream, ProviderType providerType )
        {            
            return DownloadFileAsync(providerType==ProviderType.Apprenticeship?_apprenticeshipsTemplatePath:_templatePath, stream);
        }
    }
}
