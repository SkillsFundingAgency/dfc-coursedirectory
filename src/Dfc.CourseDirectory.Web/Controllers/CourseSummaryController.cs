using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.CourseSummary;
using Microsoft.AspNetCore.Mvc;


namespace Dfc.CourseDirectory.Web.Controllers
{
    public class CourseSummaryController : Controller
    {
        private readonly ICourseService _courseService;

        public CourseSummaryController(
            ICourseService courseService
            )
        {
            Throw.IfNull(courseService, nameof(courseService));
            _courseService = courseService;

        }
        public IActionResult Index(Guid? courseId, Guid? courseRunId)
        {
            ICourse course = null;

            if (courseId.HasValue)
            {
                course = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value)).Result.Value;
            }

            CourseSummaryViewModel vm = new CourseSummaryViewModel();

            return View();
        }
    }
}
