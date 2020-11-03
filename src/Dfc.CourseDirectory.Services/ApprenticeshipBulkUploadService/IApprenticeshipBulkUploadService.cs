using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Auth;

namespace Dfc.CourseDirectory.Services.ApprenticeshipBulkUploadService
{
    public interface IApprenticeshipBulkUploadService
    {
        Task<ApprenticeshipBulkUploadResult> ValidateAndUploadCSV(string fileName, Stream stream, AuthUserDetails userDetails);
    }
}
