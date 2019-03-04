using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers.PublishCourses
{

    public class PublishCoursesController : Controller
    {
        private readonly ILogger<PublishCoursesController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;

        public PublishCoursesController(ILogger<PublishCoursesController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
        }

        [Authorize]
        public IActionResult Index()
        {
           PublishViewModel vm = new PublishViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            //ICourseSearchResult result = (!UKPRN.HasValue ? null : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN)).Result.Value);

            List<Course> Courses;

            Courses = new List<Course>()
            {
                new Course()
                {
                    CourseDescription = "Course Description 1",
                    id =Guid.NewGuid(),
                    QualificationCourseTitle = "Test Qualification 1",
                    LearnAimRef = "Test Lars Ref 1",
                    NotionalNVQLevelv2 = "Test Level 1",
                    AwardOrgCode = "Test Award Code 1",
                    CourseRuns = new List<CourseRun>()
                    {
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 1",
                            RecordStatus = RecordStatus.Live

                        },
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 2",
                            RecordStatus = RecordStatus.Live

                        },
                    }
                },
                new Course()
                {
                    CourseDescription = "Course Description 2",
                    id =Guid.NewGuid(),
                    QualificationCourseTitle = "Test Qualification 2",
                    LearnAimRef = "Test Lars Ref 2",
                    NotionalNVQLevelv2 = "Test Level 2",
                    AwardOrgCode = "Test Award Code 2",

                    CourseRuns = new List<CourseRun>()
                    {
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 3",
                            RecordStatus = RecordStatus.Live

                        },
                        new CourseRun()
                        {
                            id = Guid.NewGuid(),
                            CourseName = "Test Course Name 4",
                            RecordStatus = RecordStatus.Live

                        },
                    }
                }
            };

            vm.NumberOfCoursesInFiles = 10;
            vm.Courses = Courses;

            return View("Index", vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(PublishViewModel vm)
        {

            //Uncomment to add Courses

            //vm.Courses = vm.Courses.Select( c => {
            //    c.IsValid = true;
            //    return c;
            //}).ToList();


            //foreach (var course in vm.Courses)
            //{
            //    foreach (var courseRuns in course.CourseRuns)
            //    {
            //        courseRuns.RecordStatus = RecordStatus.Live;
            //    }
            //    var result = await _courseService.AddCourseAsync(course);

            //    if (result.IsSuccess && result.HasValue)
            //    {
            //        successfulPublishedCourses++;
            //    }

            //}

            int successfulPublishedCourses = 0;
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel()
            {
                NumberOfCoursesPublished = successfulPublishedCourses
            };

            return View("Complete", CompleteVM);
        }

    }
}