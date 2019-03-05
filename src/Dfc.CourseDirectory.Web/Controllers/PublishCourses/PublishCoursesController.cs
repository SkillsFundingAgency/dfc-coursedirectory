﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        [HttpGet]
        public IActionResult Index(PublishMode publishMode)
        {

            PublishViewModel vm = new PublishViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            List<Course> Courses = new List<Course>();

            switch (publishMode)
            {
                case PublishMode.Migration:
                    //TODO replace with call to service to return by status
                    vm.PublishMode = PublishMode.Migration;
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
                                RecordStatus = RecordStatus.MigrationPending

                            },
                            new CourseRun()
                            {
                                id = new Guid("65b03e0c-1c47-4995-a0f8-efb2739c3008"),
                                CourseName = "Test Course Name 2",
                                RecordStatus = RecordStatus.MigrationReadyToGoLive

                            },
                        }
                    },
                        new Course()
                        {
                            CourseDescription = "Course Description 2",
                            id = new Guid("4e88a520-45c2-4dc2-be57-1f6b36f2c07f"),
                            QualificationCourseTitle = "Test Qualification 2",
                            LearnAimRef = "Test Lars Ref 2",
                            NotionalNVQLevelv2 = "Test Level 2",
                            AwardOrgCode = "Test Award Code 2",
                            IsValid = false,
                            CourseRuns = new List<CourseRun>()
                            {
                                new CourseRun()
                                {
                                    id = new Guid("4aaf651d-d4df-4eb6-8b1e-982924752ecc"),
                                    CourseName = "Test Course Name 3",
                                    RecordStatus = RecordStatus.MigrationPending

                                },
                                new CourseRun()
                                {
                                    id = new Guid("469b6253-7856-4d2a-b151-3387f2718c7f"),
                                    CourseName = "Test Course Name 4",
                                    RecordStatus = RecordStatus.MigrationReadyToGoLive

                                },
                            }
                        }
                    };

                    vm.NumberOfCoursesInFiles = 10;

                    break;
                case PublishMode.BulkUpload:

                    //TODO replace with call to service to return by status
                    vm.PublishMode = PublishMode.BulkUpload;
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

                    break;
            }

            vm.Courses = Courses;

            return View("Index", vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(PublishViewModel vm)
        {
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            //Archive any existing courses
            var archiveExistingCourse = await _courseService.ArchiveProviderLiveCourses(UKPRN);

            foreach (var course in vm.Courses)
            {
                course.IsValid = true;

                foreach (var courseRuns in course.CourseRuns)
                {
                    courseRuns.RecordStatus = RecordStatus.Live;
                }
                var result = await _courseService.AddCourseAsync(course);

                if (result.IsSuccess && result.HasValue)
                {
                    CompleteVM.NumberOfCoursesPublished++;
                }
            }
            return View("Complete", CompleteVM);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Delete(Guid courseId, Guid courseRunId)
        {
            //TODO delete

            return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration });
        }
    }
}