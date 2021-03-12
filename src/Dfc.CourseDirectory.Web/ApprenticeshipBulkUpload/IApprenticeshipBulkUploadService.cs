using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;

namespace Dfc.CourseDirectory.Web.ApprenticeshipBulkUpload
{
    public interface IApprenticeshipBulkUploadService
    {
        Task<ApprenticeshipBulkUploadResult> ValidateAndUploadCSV(string fileName, Stream stream, AuthenticatedUserInfo userInfo, int ukprn);
    }
}
