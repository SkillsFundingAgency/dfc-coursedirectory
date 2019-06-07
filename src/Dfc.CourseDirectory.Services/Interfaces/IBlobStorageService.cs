
using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Interfaces;


namespace Dfc.CourseDirectory.Services.Interfaces.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task DownloadFileAsync(string filePath, Stream stream);
        Task UploadFileAsync(string filePath, Stream stream);
    }
}
