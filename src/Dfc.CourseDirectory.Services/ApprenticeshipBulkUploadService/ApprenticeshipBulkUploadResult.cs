using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Services.ApprenticeshipBulkUploadService
{
    public class ApprenticeshipBulkUploadResult
    {
        private ApprenticeshipBulkUploadResult()
        {
        }

        public IReadOnlyCollection<string> Errors { get; private set; }

        public bool ProcessedSynchronously { get; private set; }

        public static ApprenticeshipBulkUploadResult Failed(IEnumerable<string> errors) => new ApprenticeshipBulkUploadResult()
        {
            Errors = errors.ToList()
        };

        public static ApprenticeshipBulkUploadResult Success(bool processedSynchronously) => new ApprenticeshipBulkUploadResult()
        {
            Errors = Array.Empty<string>(),
            ProcessedSynchronously = processedSynchronously
        };
    }
}
