
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderCourses
{
    public class ProviderCoursesViewModel
    {
        public bool HasFilters { get; set; }
        public IList<ProviderCourseRunViewModel> ProviderCourseRuns { get; set;}

        public List<ProviderCoursesFilterItemModel> Levels { get; set; }
        public List<ProviderCoursesFilterItemModel> DeliveryModes { get; set; }
    }
}
