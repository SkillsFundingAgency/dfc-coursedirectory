using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IApprenticeshipProvisionHelper
    {
        Task<FileStreamResult> DownloadCurrentApprenticeshipProvisions();
    }
}
