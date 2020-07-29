using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Auth;

namespace Dfc.CourseDirectory.Services.Interfaces.BulkUploadService
{
    public class ApprenticeshipBulkUploadResult
    {
        private ApprenticeshipBulkUploadResult()
        {
        }

        public IReadOnlyCollection<string> Errors { get; private set; }

        public static ApprenticeshipBulkUploadResult Failed(IEnumerable<string> errors) => new ApprenticeshipBulkUploadResult()
        {
            Errors = errors.ToList()
        };

        public static ApprenticeshipBulkUploadResult Success() => new ApprenticeshipBulkUploadResult()
        {
            Errors = Array.Empty<string>()
        };
    }

    public interface IApprenticeshipBulkUploadService
    {
        int CountCsvLines(Stream stream);
        Task<ApprenticeshipBulkUploadResult> ValidateAndUploadCSV(
            string fileName,
            Stream stream,
            AuthUserDetails userDetails,
            bool processInline);
    }
}
