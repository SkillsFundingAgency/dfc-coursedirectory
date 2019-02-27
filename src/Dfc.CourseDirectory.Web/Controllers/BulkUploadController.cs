using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{

    public class BulkUploadController : Controller
    {
        private readonly ILogger<BulkUploadController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;

        public BulkUploadController(ILogger<BulkUploadController> logger,
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
            var model = new BulkUploadViewModel();
            model.AbraKadabra = "Welcome to BulkUpload UI! <br /> Get ready to be amazed!";

            return View("Index", model);
        }

        [Authorize]
        public IActionResult Publish()
        {
            BulkUploadPublishViewModel vm = new BulkUploadPublishViewModel();

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

            vm.Courses = Courses;

            return View("Publish", vm);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Publish(BulkUploadPublishViewModel vm)
        {
            //Do stuff

            return RedirectToAction("Courses", "Provider", new { qualificationType = "", courseId = Guid.NewGuid(), courseRunId = Guid.NewGuid() });
        }
    }
}