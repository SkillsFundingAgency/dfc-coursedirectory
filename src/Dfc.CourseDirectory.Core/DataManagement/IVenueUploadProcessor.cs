using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public interface IVenueUploadProcessor
    {
        Task ProcessFile(Guid venueUploadId, Stream stream);
        Task<SaveFileResult> SaveFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task WaitForProcessingToComplete(Guid venueUploadId, CancellationToken cancellationToken);
    }
}
