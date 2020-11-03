using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Auth;

namespace Dfc.CourseDirectory.Services.Interfaces.BulkUploadService
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

    public interface IApprenticeshipBulkUploadService
    {
        Task<ApprenticeshipBulkUploadResult> ValidateAndUploadCSV(string fileName, Stream stream, AuthUserDetails userDetails);
    }
}
