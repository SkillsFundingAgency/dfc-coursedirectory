using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public interface IVenueUploadProcessor
    {
        IObservable<UploadStatus> GetUploadStatusUpdates(Guid venueUploadId);
        Task ProcessFile(Guid venueUploadId, Stream stream);
        Task<SaveFileResult> SaveFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task WaitForProcessingToComplete(Guid venueUploadId, CancellationToken cancellationToken);
    }
}
