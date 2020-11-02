using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
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
        private readonly HttpClient _httpClient;

        private readonly string _accountName;
        private readonly string _accountKey;
        //private readonly string _containerName;
        //private readonly string _bulkUploadPathFormat;
        private readonly string _templatePath;
        private readonly string _apprenticeshipsTemplatePath;

        private readonly CloudStorageAccount _account;
        //private readonly CloudBlobClient _blobClient;
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
            _httpClient = httpClient;

            //_getSomethingByIdUri = settings.Value.ToGetSomethingByIdUri();
            _accountName = settings.Value.AccountName;
            _accountKey = settings.Value.AccountKey;
            //_containerName = settings.Value.Container;
            //_bulkUploadPathFormat = settings.Value.BulkUploadPathFormat;
            _templatePath = settings.Value.TemplatePath;
            _apprenticeshipsTemplatePath = settings.Value.ApprenticeshipsTemplatePath;

            //Set up the client
            _account = new CloudStorageAccount(new StorageCredentials(_accountName, _accountKey), true);
            //_blobClient = _account.CreateCloudBlobClient();
            _container = _account.CreateCloudBlobClient()
                                 .GetContainerReference(settings.Value.Container);

            _inlineProcessingThreshold = settings.Value.InlineProcessingThreshold;
            if(default(int) == _inlineProcessingThreshold)
            {
                _inlineProcessingThreshold = 10000; // if no setting then set the threshold really high so that all processing is inline
            }
        }

        public Task DownloadFileAsync(string filePath, Stream stream)
        {
            try {
                CloudBlockBlob blockBlob = _container.GetBlockBlobReference(filePath);

                if (blockBlob.ExistsAsync().Result) {
                    _log.LogInformation($"Downloading {filePath} from blob storage");
                    return blockBlob.DownloadToStreamAsync(stream);
                } else {
                    return null;
                }

            //} catch (StorageException stex) {
            //    _log.LogException($"Exception downloading {filePath}", stex);
            //    return null;

            } catch (Exception ex) {
                _log.LogException($"Exception downloading {filePath}", ex);
                return null;
            }
        }

        public Task UploadFileAsync(string filePath, Stream stream)
        {
            try {
                CloudBlockBlob blockBlob = _container.GetBlockBlobReference(filePath);
                var p = string.Join("/", filePath.Split("/").Reverse().Skip(1).Reverse());
                ArchiveFiles(p);

                stream.Position = 0;
                return blockBlob.UploadFromStreamAsync(stream);

            //} catch (StorageException stex) {
            //    _log.LogException($"Exception downloading {filePath}", stex);
            //    return null;

            } catch (Exception ex) {
                _log.LogException($"Exception uploading {filePath}", ex);
                return null;
            }
        }

        public IEnumerable<BlobFileInfo> GetFileList(string filePath)
        {
            try {
                return _container.GetDirectoryReference(filePath)
                                ?.ListBlobs()
                                ?.OfType<CloudBlockBlob>()
                                ?.Select(b => new BlobFileInfo() { Name = b.Name, Size = b.Properties.Length, DateUploaded = b.Properties.Created });

                //} catch (StorageException stex) {
                //    _log.LogException($"Exception listing files at {filePath}", stex);
                //    return null;

            } catch (Exception ex) {
                _log.LogException($"Exception listing files at {filePath}", ex);
                return null;
            }
        }

        public IEnumerable<CloudBlockBlob> ArchiveFiles(string filePath)
        {
            try {                
                IEnumerable<CloudBlockBlob> blobs = _container?.GetDirectoryReference(filePath)
                                                              ?.ListBlobs()
                                                              ?.OfType<CloudBlockBlob>()
                                                    ?? new CloudBlockBlob[] { };
                foreach(CloudBlockBlob b in blobs)
                {
                    _container.GetBlobReference($"Archive/{filePath.Substring(0, 8)}_{b.Uri.Segments.Last()}")
                              .StartCopy(b.Uri);
                    b.DeleteIfExists();
                }

                return blobs;

                //} catch (StorageException stex) {
                //    _log.LogException($"Exception archiving files at {filePath}", stex);
                //    return null;

            }
            catch (Exception ex)
            {
                _log.LogException($"Exception archiving files at {filePath}", ex);
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
