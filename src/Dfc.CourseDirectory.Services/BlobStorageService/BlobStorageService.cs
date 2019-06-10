
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;


namespace Dfc.CourseDirectory.Services.BlobStorageService
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly ILogger<BlobStorageService> _log;
        private readonly HttpClient _httpClient;

        private readonly string _accountName;
        private readonly string _accountKey;
        private readonly string _container;
        //private readonly string _bulkUploadPathFormat;
        private readonly string _templatePath;

        private readonly CloudStorageAccount _account;
        private readonly CloudBlobClient _blobClient;

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
            _container = settings.Value.Container;
            //_bulkUploadPathFormat = settings.Value.BulkUploadPathFormat;
            _templatePath = settings.Value.TemplatePath;

            //Set up the client
            _account = new CloudStorageAccount(new StorageCredentials(_accountName, _accountKey), true);
            _blobClient = _account.CreateCloudBlobClient();
        }

        public Task DownloadFileAsync(string filePath, Stream stream)
        {
            try {
                //CloudBlobContainer container = _blobClient.GetContainerReference(_container);
                CloudBlockBlob blockBlob = _blobClient.GetContainerReference(_container)
                                                      .GetBlockBlobReference(filePath);

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
                //CloudBlobContainer container = _blobClient.GetContainerReference(_container);
                CloudBlockBlob blockBlob = _blobClient.GetContainerReference(_container)
                                                      .GetBlockBlobReference(filePath);

                return blockBlob.UploadFromStreamAsync(stream);

            //} catch (StorageException stex) {
            //    _log.LogException($"Exception downloading {filePath}", stex);
            //    return null;

            } catch (Exception ex) {
                _log.LogException($"Exception uploading {filePath}", ex);
                return null;
            }
        }

        public Task GetBulkUploadTemplateFileAsync(Stream stream)
        {
            return DownloadFileAsync(_templatePath, stream);
        }

    }
}
