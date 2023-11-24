using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Core.BinaryStorageProvider
{
    public class BlobStorageBinaryStorageProvider : IBinaryStorageProvider
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ILogger<BlobStorageBinaryStorageProvider> _log;
        private bool _containerExists;

        public BlobStorageBinaryStorageProvider(IOptions<BlobStorageBinaryStorageProviderSettings> options, ILogger<BlobStorageBinaryStorageProvider> log)
        {
            _blobContainerClient = new BlobContainerClient(options.Value.ConnectionString, options.Value.ContainerName);
            _log = log;
        }

        public async Task<bool> TryDownloadFile(string path, Stream destination)
        {
            _log.LogInformation($"Start Downloading a File from [{path}]");
            if (string.IsNullOrEmpty(path))
            {
                _log.LogError($"Failed to Download a file. File Path can not be empty.");
                throw new ArgumentException("Path cannot be empty.", nameof(path));
            }

            if (destination == null)
            {
                _log.LogError($"Failed to Download a file. Destination stream can not be empty.");
                throw new ArgumentNullException(nameof(destination));
            }

            await EnsureContainerExists();

            var blob = _blobContainerClient.GetBlobClient(path);

            try
            {
                await blob.DownloadToAsync(destination);
                _log.LogInformation($"Complete Download and saved in a stream.");
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == (int)System.Net.HttpStatusCode.NotFound)
            {
                _log.LogWarning($"Failed to Download a file and Response Code [{ex.Status}]");
                return false;
            }
        }

        public async Task UploadFile(string path, Stream source)
        {
            _log.LogInformation($"Start Uploading a File from a datastream.");

            if (string.IsNullOrEmpty(path))
            {
                _log.LogError($"Failed to Upload a file. Destination path can not be empty.");
                throw new ArgumentException("Path cannot be empty.", nameof(path));
            }

            if (source == null)
            {
                _log.LogError($"Failed to Upload. Source stream can not be empty.");
                throw new ArgumentNullException(nameof(source));
            }

            await EnsureContainerExists();

            var blob = _blobContainerClient.GetBlobClient(path);
            await blob.UploadAsync(source);

            _log.LogInformation($"File Upload Complete and Saved in [{path}]");
        }

        public async Task<IReadOnlyCollection<BlobFileInfo>> ListFiles(string path)
        {
            _log.LogInformation($"Start Getting list of files from path [{path}].");
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be empty.", nameof(path));
            }

            await EnsureContainerExists();

            var files = new List<BlobFileInfo>();

            await foreach (var blobItem in _blobContainerClient.GetBlobsAsync(prefix: path))
            {
                files.Add(new BlobFileInfo
                {
                    Name = blobItem.Name,
                    Size = blobItem.Properties.ContentLength,
                    DateUploaded = blobItem.Properties.CreatedOn
                });
            }

            _log.LogInformation($"[{files.Count}] files retrieved from given path [{path}]");
            return files;
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
