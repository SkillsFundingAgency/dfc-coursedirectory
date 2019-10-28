using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IApprenticeshipProvisionHelper
    {
        FileStreamResult DownloadCurrentApprenticeshipProvisions();
    }
}
