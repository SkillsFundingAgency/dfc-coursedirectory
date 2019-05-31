using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IFileHelper
    {
        Task<bool> DownloadFile(string blobFileName, string folder);
        Task<bool> UploadFile(IFormFile blobFileName, string folder);
    }
}
