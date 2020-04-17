using System.Collections.Generic;
using System.IO;
using Dfc.CourseDirectory.Models.Models.Auth;

namespace Dfc.CourseDirectory.Services.Interfaces.BulkUploadService
{
    public interface IApprenticeshipBulkUploadService
    {
        int CountCsvLines(Stream stream);
        List<string> ValidateAndUploadCSV(Stream stream, AuthUserDetails userDetails, bool updateApprenticeships);
    }
}
