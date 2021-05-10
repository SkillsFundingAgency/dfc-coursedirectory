using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public interface IFileUploadProcessor
    {
        IObservable<UploadStatus> GetVenueUploadStatusUpdates(Guid venueUploadId);
        Task ProcessVenueFile(Guid venueUploadId, Stream stream);
        Task<SaveFileResult> SaveVenueFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task WaitForVenueProcessingToComplete(Guid venueUploadId, CancellationToken cancellationToken);

        Task<SaveFileResult> SaveCourseFile(Guid providerId, Stream stream, UserInfo uploadedBy);
    }
}
