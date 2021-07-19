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
        // Courses
        Task DeleteCourseUploadForProvider(Guid providerId);
        Task<UploadStatus> DeleteCourseUploadRowForProvider(Guid providerId, int rowNumber);
        Task<UploadStatus> DeleteCourseUploadRowGroupForProvider(Guid providerId, Guid courseId);
        Task<CourseUploadRowDetail> GetCourseUploadRowDetailForProvider(Guid providerId, int rowNumber);
        Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowGroupForProvider(Guid providerId, Guid courseId);
        Task<(IReadOnlyCollection<CourseUploadRow> Rows, UploadStatus UploadStatus)> GetCourseUploadRowsForProvider(Guid providerId);
        Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowsWithErrorsForProvider(Guid providerId);
        IObservable<UploadStatus> GetCourseUploadStatusUpdatesForProvider(Guid providerId);
        Task ProcessCourseFile(Guid courseUploadId, Stream stream);
        Task<SaveFileResult> SaveCourseFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task<UploadStatus> UpdateCourseUploadRowForProvider(Guid providerId, int rowNumber, CourseUploadRowUpdate update);
        Task<UploadStatus> UpdateCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, CourseUploadRowGroupUpdate update);

        // Venues
        Task DeleteVenueUploadForProvider(Guid providerId);
        Task<UploadStatus> DeleteVenueUploadRowForProvider(Guid providerId, int rowNumber);
        Task<(IReadOnlyCollection<VenueUploadRow> Rows, UploadStatus UploadStatus)> GetVenueUploadRowsForProvider(Guid providerId);
        IObservable<UploadStatus> GetVenueUploadStatusUpdatesForProvider(Guid providerId);
        Task ProcessVenueFile(Guid venueUploadId, Stream stream);
        Task<PublishResult> PublishVenueUploadForProvider(Guid providerId, UserInfo publishedBy);
        Task<SaveFileResult> SaveVenueFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task<UploadStatus> UpdateVenueUploadRowForProvider(Guid providerId, int rowNumber, CsvVenueRow updatedRow);
        Task<UploadStatus> WaitForVenueProcessingToCompleteForProvider(Guid providerId, CancellationToken cancellationToken);

        // Apprenticeships
        Task<SaveFileResult> SaveApprenticeshipFile(Guid providerId, Stream stream, UserInfo uploadedBy);
    }
}
