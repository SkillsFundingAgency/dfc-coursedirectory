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
        public IActionResult Index(PublishMode publishMode, string notificationTitle, Guid? courseId, Guid? courseRunId)
        {

            PublishViewModel vm = new PublishViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            List<Course> Courses = new List<Course>();

            ICourseSearchResult coursesByUKPRN = (!UKPRN.HasValue
                   ? null
                   : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                       .Result.Value);
            Courses = coursesByUKPRN.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

            switch (publishMode)
            {
                case PublishMode.Migration:
                    //TODO replace with call to service to return by status
                    vm.PublishMode = PublishMode.Migration;
                    var migratedCourses = Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.MigrationPending || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive)).ToList();
                    vm.NumberOfCoursesInFiles = migratedCourses.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.MigrationPending || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive)).Count();
                    vm.Courses = migratedCourses.OrderBy(x=>x.QualificationCourseTitle);
                    vm.AreAllReadyToBePublished = CheckAreAllReadyToBePublished(migratedCourses, PublishMode.Migration);

                    break;
                case PublishMode.BulkUpload:

                    //TODO replace with call to service to return by status
                    vm.PublishMode = PublishMode.BulkUpload;
                    var bulkUploadedCourses = Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.BulkUploadPending || cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).ToList();
                    vm.NumberOfCoursesInFiles = bulkUploadedCourses.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.BulkUploadPending || cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).Count();
                    vm.Courses = bulkUploadedCourses.OrderBy(x => x.QualificationCourseTitle);
                    vm.AreAllReadyToBePublished = CheckAreAllReadyToBePublished(bulkUploadedCourses, PublishMode.BulkUpload);

                    break;
                case PublishMode.DataQualityIndicator:
                    {

                        vm.PublishMode = PublishMode.DataQualityIndicator;
                        var validCourses = Courses.Where(x => x.IsValid);
                        var results = _courseService.CourseValidationMessages(validCourses, ValidationMode.DataQualityIndicator).Value.ToList();
                        var invalidCoursesResult = results.Where(c => c.RunValidationResults.Any(cr => cr.Issues.Count() > 0));
                        var invalidCourses = invalidCoursesResult.Select(c => (Course)c.Course).ToList();
                        var courseRuns = invalidCourses.Select(cr => cr.CourseRuns.Where(x => x.StartDate < DateTime.Today));

                        List<Course> filteredList = new List<Course>();
                        foreach(var course in invalidCourses)
                        {
                            var invalidRuns = course.CourseRuns.Where(x => x.StartDate < DateTime.Today);

                            if (invalidRuns.Count() != 0)
                            {
                                course.CourseRuns = invalidRuns;
                                filteredList.Add(course);
                            }

                        }

                        if(courseRuns.Count() == 0 && courseId != null && courseRunId != null)
                        {
                            var dashboardVm = DashboardController.GetDashboardViewModel(_courseService, _session.GetInt32("UKPRN"), notificationTitle);
                            return RedirectToAction("IndexSuccess", "Home", dashboardVm);
                        }


                    vm.NumberOfCoursesInFiles = invalidCourses.Count();
                        vm.Courses = filteredList.OrderBy(x => x.QualificationCourseTitle);
                        break;
                    }
            }

            vm.NotificationTitle = notificationTitle;
            vm.CourseId = courseId;
            vm.CourseRunId = courseRunId;

            return View("Index", vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(PublishViewModel vm)
        {
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel();

            int? sUKPRN = _session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            else
            {
                UKPRN = sUKPRN ?? 0;
            }

            CompleteVM.NumberOfCoursesPublished = vm.NumberOfCoursesInFiles; 

            switch (vm.PublishMode)
            {
                case PublishMode.Migration:

                    // Publish migrated courses directly, NO archiving
                    var resultPublishMigratedCourses = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationReadyToGoLive, (int)RecordStatus.Live);
                    CompleteVM.Mode = PublishMode.Migration;
                    if (resultPublishMigratedCourses.IsSuccess)
                    {
                        return View("Complete", CompleteVM);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home", new { errmsg = "Publish All Migration-PublishCourses Error" });
                    }

                case PublishMode.BulkUpload:

                    //Archive any existing courses
                    var resultArchivingCourses = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.Live, (int)RecordStatus.Archived);
                    if (resultArchivingCourses.IsSuccess)
                    {
                        // Publish courses
                        var resultPublishBulkUploadedCourses = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);
                        CompleteVM.Mode = PublishMode.BulkUpload;
                        if (resultPublishBulkUploadedCourses.IsSuccess)
                        {
                            return View("Complete", CompleteVM);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-PublishCourses Error" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-ArchiveCourses Error" });
                    }

                default:
                    // TODO: We should have generic error handling page
                    return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload/Migration Error" });
            }            
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid courseId, Guid courseRunId,string courseName)
        {
            //TODO delete
            string notificationTitle = string.Empty;

            var result = await _courseService.UpdateStatus(courseId, courseRunId, (int)RecordStatus.Deleted);

            if (result.IsSuccess)
            {
                //do something
                notificationTitle = courseName + " was successfully deleted";
            }
            else
            {
                notificationTitle = "Error " + courseName + " was not deleted";
            }

            return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration,notificationTitle = notificationTitle });
        }

        public bool CheckAreAllReadyToBePublished(List<Course> courses, PublishMode publishMode)
        {
            bool AreAllReadyToBePublished = false;

            if(courses.Count.Equals(0))
                return AreAllReadyToBePublished;

            var hasInvalidCourses = courses.Any(c => c.IsValid == false);
            if (hasInvalidCourses)
            {
                return AreAllReadyToBePublished;
            }
            else
            {
                switch (publishMode)
                {
                    case PublishMode.Migration:
                        var hasInvalidMigrationCourseRuns = courses.Any(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.MigrationPending));
                        if (!hasInvalidMigrationCourseRuns)
                            AreAllReadyToBePublished = true;
                        break;
                    case PublishMode.BulkUpload:
                        var hasInvalidBulkUploadCourseRuns = courses.Any(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.BulkUploadPending));
                        if (!hasInvalidBulkUploadCourseRuns)
                            AreAllReadyToBePublished = true;
                        break;
                }
                return AreAllReadyToBePublished;
            }         
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> WhatDoYouWantToDoNext()
        {
            var model = new WhatDoYouWantToDoNextViewModel();

            return View("../WhatDoYouWantToDoNext/Index",model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> WhatDoYouWantToDoNext(WhatDoYouWantToDoNextViewModel model)
        {
            switch(model.WhatDoYouWantToDoNext)
            {
                case Models.Enums.WhatDoYouWantToDoNext.OnScreen:
                    return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.BulkUpload });
                case Models.Enums.WhatDoYouWantToDoNext.DownLoad:
                    return RedirectToAction("DownloadErrorFile", "PublishCourses");
                case Models.Enums.WhatDoYouWantToDoNext.Delete:
                    return RedirectToAction("DeleteFile", "PublishCourses");
                default:
                    return RedirectToAction("WhatDoYouWantToDoNext", "PublishCourses");

            }


            
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DownloadErrorFile()
        {
            var model = new DownloadErrorFileViewModel();
            model.ErrorFileCreatedDate = DateTime.Now;

            return View("../DownloadErrorFile/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DownloadErrorFile(DownloadErrorFileViewModel model)
        {
            // where to go????
            return View("../WhatDoYouWantToDoNext/Index", new WhatDoYouWantToDoNextViewModel());
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteFile()
        {
            var model = new DeleteFileViewModel();
            

            return View("../DeleteFile/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteFile(DeleteFileViewModel model)
        {
            // where to go????
            return RedirectToAction("DeleteFileConfirmation", "PublishCourses");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteFileConfirmation()
        {
            var model = new DeleteFileConfirmationViewModel();
            model.FileUploadedDate = DateTime.Now;

            return View("../DeleteFileConfirmation/Index", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PublishYourFile()
        {
            var model = new PublishYourFileViewModel();

            model.NumberOfCourses = 99;

            return View("../PublishYourFile/Index", model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PublishYourFile(PublishYourFileViewModel model)
        {
            //to publish stuff
            return View("Complete", new PublishCompleteViewModel() { NumberOfCoursesPublished = 99, Mode = PublishMode.BulkUpload });
        }








    }
}