using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum UploadStatus
    {
        /// <summary>
        /// A file has been uploaded but processing its contents has not started.
        /// </summary>
        Created = 0,

        /// <summary>
        /// Processing of the upload's contents is currently in progress.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Processing the upload's contents completed with validation errors.
        /// </summary>
        ProcessedWithErrors = 2,

        /// <summary>
        /// Processing the upload's contents completed without validation errors.
        /// </summary>
        ProcessedSuccessfully = 3,

        /// <summary>
        /// The contents of the upload have been published.
        /// </summary>
        Published = 4,

        /// <summary>
        /// The upload has been replaced with another without its contents published.
        /// </summary>
        Abandoned = 5
    }

    public static class UploadStatusExtensions
    {
        public static IReadOnlyCollection<UploadStatus> UnpublishedStatuses { get; } = new[]
        {
            UploadStatus.Created,
            UploadStatus.Processing,
            UploadStatus.ProcessedWithErrors,
            UploadStatus.ProcessedSuccessfully
        };

        public static bool IsTerminal(this UploadStatus status) => status switch
        {
            UploadStatus.Published => true,
            UploadStatus.Abandoned => true,
            _ => false
        };

        public static bool IsUnprocessed(this UploadStatus status) => status switch
        {
            UploadStatus.Created => true,
            UploadStatus.Processing => true,
            _ => false
        };
    }
}
