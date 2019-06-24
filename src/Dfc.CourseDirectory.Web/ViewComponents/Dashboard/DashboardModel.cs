using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Dashboard
{
    public class DashboardModel
    {
        public int PublishedCourseCount { get; set; }
        public int VenueCount { get; set; }
    }
}
