using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
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
                    id = new Guid("24893c87-bec3-48d8-9647-cca87ec6ec51"),
                    QualificationCourseTitle = "Test Qualification 1",
                    LearnAimRef = "Test Lars Ref 1",
                    NotionalNVQLevelv2 = "Test Level 1",
                    AwardOrgCode = "Test Award Code 1",
                    IsValid = true,
                    CourseRuns = new List<CourseRun>()
                    {
                        new CourseRun()
                        {
                            id = new Guid("65b03e0c-1c47-4995-a0f8-efb2739c3008"),
                            CourseName = "Test Course Name 1",
                            RecordStatus = RecordStatus.BulkUloadPending

                        },
                        new CourseRun()
                        {
                            id = new Guid("65b03e0c-1c47-4995-a0f8-efb2739c3008"),
                            CourseName = "Test Course Name 2",
                            RecordStatus = RecordStatus.BulkUploadReadyToGoLive

                        },
                    }
                },
                new Course()
                {
                    CourseDescription = "Course Description 2",
                    id = new Guid("24893c87-bec3-48d8-9647-cca87ec6ec51"),
                    QualificationCourseTitle = "Test Qualification 2",
                    LearnAimRef = "Test Lars Ref 2",
                    NotionalNVQLevelv2 = "Test Level 2",
                    AwardOrgCode = "Test Award Code 2",
                    IsValid = false,
                    CourseRuns = new List<CourseRun>()
                    {
                        new CourseRun()
                        {
                            id = new Guid("65b03e0c-1c47-4995-a0f8-efb2739c3008"),
                            CourseName = "Test Course Name 3",
                            RecordStatus = RecordStatus.BulkUloadPending

                        },
                        new CourseRun()
                        {
                            id = new Guid("65b03e0c-1c47-4995-a0f8-efb2739c3008"),
                            CourseName = "Test Course Name 4",
                            RecordStatus = RecordStatus.BulkUploadReadyToGoLive

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
        public IActionResult Index(PublishViewModel vm)
        {
            //TODO publish



            //TODO replace with result from publish?
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel()
            {
                NumberOfCoursesPublished = 10
            };

            return View("Complete", CompleteVM);
        }
    }
}