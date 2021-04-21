using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public interface ICourseUploadProcessor
    {
        Task<SaveFileResult> SaveFile(Guid providerId, Stream stream, UserInfo uploadedBy);
    }
}
