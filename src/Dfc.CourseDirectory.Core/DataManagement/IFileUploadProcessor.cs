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
        Task DeleteCourseUploadForProvider(Guid providerId, bool isNonLars);
        Task<UploadStatus> DeleteCourseUploadRowForProvider(Guid providerId, int rowNumber, bool isNonLars);
        Task<UploadStatus> DeleteCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, bool isNonLars);
        Task<CourseUploadRowDetail> GetCourseUploadRowDetailForProvider(Guid providerId, int rowNumber, bool isNonLars);
        Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, bool isNonLars);
        Task<(IReadOnlyCollection<CourseUploadRow> Rows, UploadStatus UploadStatus)> GetCourseUploadRowsForProvider(Guid providerId, bool isNonLars);
        Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowsWithErrorsForProvider(Guid providerId, bool isNonLars);
        IObservable<UploadStatus> GetCourseUploadStatusUpdatesForProvider(Guid providerId, bool isNonLars);
        Task ProcessCourseFile(Guid courseUploadId, Stream stream);
        Task<PublishResult> PublishCourseUploadForProvider(Guid providerId, UserInfo publishedBy, bool IsNonLars);
        Task<SaveCourseFileResult> SaveCourseFile(Guid providerId,bool isNonLars, Stream stream, UserInfo uploadedBy);
        Task<UploadStatus> UpdateCourseUploadRowForProvider(Guid providerId, int rowNumber, bool isNonLars, CourseUploadRowUpdate update);
        Task<UploadStatus> UpdateCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, CourseUploadRowGroupUpdate update, bool isNonLars);

        // Venues
        Task DeleteVenueUploadForProvider(Guid providerId);
        Task<UploadStatus> DeleteVenueUploadRowForProvider(Guid providerId, int rowNumber);
        Task<(IReadOnlyCollection<VenueUploadRow> Rows, UploadStatus UploadStatus)> GetVenueUploadRowsForProvider(Guid providerId);
        IObservable<UploadStatus> GetVenueUploadStatusUpdatesForProvider(Guid providerId);
        Task ProcessVenueFile(Guid venueUploadId, Stream stream);
        Task<PublishResult> PublishVenueUploadForProvider(Guid providerId, UserInfo publishedBy);
        Task<SaveVenueFileResult> SaveVenueFile(Guid providerId, Stream stream, UserInfo uploadedBy);
        Task<UploadStatus> UpdateVenueUploadRowForProvider(Guid providerId, int rowNumber, CsvVenueRow updatedRow);
        Task<UploadStatus> WaitForVenueProcessingToCompleteForProvider(Guid providerId, CancellationToken cancellationToken);
    }
}
