using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public interface IVenueUploadProcessor
    {
        Task<SaveFileResult> SaveFile(Guid providerId, Stream stream, UserInfo uploadedBy);
    }
}
