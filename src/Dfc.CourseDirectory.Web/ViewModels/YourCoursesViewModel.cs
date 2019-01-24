
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class YourCoursesViewModel
    {
        public int? UKPRN { get; set; }

        public Guid? UpdatedCourseId { get; set; }

        public Guid? CourseRunId { get; set; }

        public ICourseSearchResult Courses { get; set; }
        public List<SelectListItem> Venues { get; set; }

        public List<SelectListItem> deliveryModes { get; set; }

        public List<SelectListItem> durationUnits { get; set; }

        public List<SelectListItem> attendances { get; set; }
        public List<SelectListItem> modes { get; set; }
    }
}
