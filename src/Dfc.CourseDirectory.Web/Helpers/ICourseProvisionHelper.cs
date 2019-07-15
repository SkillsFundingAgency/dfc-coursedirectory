using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface ICourseProvisionHelper
    {
        FileStreamResult DownloadCurrentCourseProvisions();
    }
}
