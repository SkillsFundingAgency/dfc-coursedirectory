using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public interface IFileUploadProcessor
    {
        Task<IReadOnlyCollection<VenueUploadRow>> GetVenueUploadRows(Guid venueUploadId);
        IObservable<UploadStatus> GetVenueUploadStatusUpdates(Guid venueUploadId);
        Task ProcessVenueFile(Guid venueUploadId, Stream stream);
        Task<SaveFileResult> SaveVenueFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task<UploadStatus> WaitForVenueProcessingToComplete(Guid venueUploadId, CancellationToken cancellationToken);

        Task<SaveFileResult> SaveCourseFile(Guid providerId, Stream stream, UserInfo uploadedBy);
    }
}
