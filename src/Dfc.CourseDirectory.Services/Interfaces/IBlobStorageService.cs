
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Storage.Blob;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Services.BlobStorageService;


namespace Dfc.CourseDirectory.Services.Interfaces.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task DownloadFileAsync(string filePath, Stream stream);
        Task UploadFileAsync(string filePath, Stream stream);
        IEnumerable<BlobFileInfo> GetFileList(string filePath);
        IEnumerable<CloudBlockBlob> ArchiveFiles(string filePath);
        Task GetBulkUploadTemplateFileAsync(Stream stream);
    }
}
