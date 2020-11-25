using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface ICourseProvisionHelper
    {
        Task<FileStreamResult> DownloadCurrentCourseProvisions();
    }
}
