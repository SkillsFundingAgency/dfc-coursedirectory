﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public interface IFileUploadProcessor
    {
        Task<(IReadOnlyCollection<VenueUploadRow> Rows, UploadStatus UploadStatus)> GetVenueUploadRowsForProvider(Guid providerId);
        IObservable<UploadStatus> GetVenueUploadStatusUpdatesForProvider(Guid providerId);
        Task ProcessVenueFile(Guid venueUploadId, Stream stream);
        Task<PublishResult> PublishVenueUploadForProvider(Guid providerId, UserInfo publishedBy);
        Task<SaveFileResult> SaveVenueFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task<UploadStatus> WaitForVenueProcessingToCompleteForProvider(Guid providerId, CancellationToken cancellationToken);

        Task<SaveFileResult> SaveCourseFileForProvider(Guid providerId, Stream stream, UserInfo uploadedBy);
    }
}
