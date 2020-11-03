using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Providers;
using Microsoft.Azure.Storage.Blob;

namespace Dfc.CourseDirectory.Services.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task DownloadFileAsync(string filePath, Stream stream);
        Task UploadFileAsync(string filePath, Stream stream);
        IEnumerable<BlobFileInfo> GetFileList(string filePath);
        IEnumerable<CloudBlockBlob> ArchiveFiles(string filePath);
        Task GetBulkUploadTemplateFileAsync(Stream stream);
        Task GetBulkUploadTemplateFileAsync(Stream stream,ProviderType providerType);
        int InlineProcessingThreshold { get; }
    }
}
