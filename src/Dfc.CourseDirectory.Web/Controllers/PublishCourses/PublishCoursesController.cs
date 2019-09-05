﻿
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Dfc.CourseDirectory.Web.Controllers.PublishCourses
{

    public class PublishCoursesController : Controller
    {
        private readonly ILogger<PublishCoursesController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IBlobStorageService _blobStorageService;

        public PublishCoursesController(ILogger<PublishCoursesController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService,
            IVenueService venueService,IBlobStorageService blobStorageService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(blobStorageService, nameof(blobStorageService));
            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            _blobStorageService = blobStorageService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index(PublishMode publishMode, string notificationTitle, Guid? courseId, Guid? courseRunId, bool fromBulkUpload)
        {
            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            List<Course> Courses = new List<Course>();
            ICourseSearchResult coursesByUKPRN = (!UKPRN.HasValue
                   ? null
                   : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                       .Result.Value);
            Courses = coursesByUKPRN.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();
            PublishViewModel vm = new PublishViewModel();


            switch (publishMode)
            {
                case PublishMode.Migration:
                   if (Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.MigrationPending)).Any())
                    {
                        vm.PublishMode = PublishMode.Migration;
                        var migratedCourses = Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.MigrationPending || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive)).ToList();
                        vm.NumberOfCoursesInFiles = migratedCourses.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.MigrationPending || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive)).Count();
                        vm.Courses = migratedCourses.OrderBy(x => x.QualificationCourseTitle);
                        vm.AreAllReadyToBePublished = CheckAreAllReadyToBePublished(migratedCourses, PublishMode.Migration);
                        vm.Courses = GetErrorMessages(vm.Courses, ValidationMode.MigrateCourse);
                        vm.Venues = VenueHelper.GetVenueNames(vm.Courses, _venueService).Result;
                        break;
                    }
                    else
                    {
                        return View("../Migration/Complete/index");
                    }

                case PublishMode.BulkUpload:

                    vm.PublishMode = PublishMode.BulkUpload;
                    var bulkUploadedCourses = Courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.BulkUploadPending || cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).ToList();
                    vm.NumberOfCoursesInFiles = bulkUploadedCourses.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.BulkUploadPending || cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).Count();
                    vm.Courses = bulkUploadedCourses.OrderBy(x => x.QualificationCourseTitle);
                    vm.AreAllReadyToBePublished = CheckAreAllReadyToBePublished(bulkUploadedCourses, PublishMode.BulkUpload);
                    vm.Courses = GetErrorMessages(vm.Courses, ValidationMode.BulkUploadCourse);
                    vm.Venues = VenueHelper.GetVenueNames(vm.Courses, _venueService).Result;
                    break;

                case PublishMode.DataQualityIndicator:

                    vm.PublishMode = PublishMode.DataQualityIndicator;
                    var validCourses = Courses.Where(x => x.IsValid && ((int)x.CourseStatus & (int)RecordStatus.Live) > 0);
                    var results = _courseService.CourseValidationMessages(validCourses, ValidationMode.DataQualityIndicator).Value.ToList();
                    var invalidCoursesResult = results.Where(c => c.RunValidationResults.Any(cr => cr.Issues.Count() > 0));
                    var invalidCourses = invalidCoursesResult.Select(c => (Course)c.Course).ToList();
                    var courseRuns = invalidCourses.Select(cr => cr.CourseRuns.Where(x => x.StartDate < DateTime.Today));
                    List<Course> filteredList = new List<Course>();
                    var allRegions = _courseService.GetRegions().RegionItems;
                    foreach (var course in invalidCourses)
                    {
                        var invalidRuns = course.CourseRuns.Where(x => x.StartDate < DateTime.Today);
                        if (invalidRuns.Any())
                        {
                            course.CourseRuns = invalidRuns;
                            filteredList.Add(course);
                        }
                    }

                    if(courseRuns.Count() == 0 && courseId != null && courseRunId != null)
                    {
                        var dashboardVm = DashboardController.GetDashboardViewModel(_courseService, _blobStorageService,_session.GetInt32("UKPRN"), notificationTitle);
                        return RedirectToAction("IndexSuccess", "Home", dashboardVm);
                    }

                    vm.NumberOfCoursesInFiles = invalidCourses.Count();
                    vm.Courses = filteredList.OrderBy(x => x.QualificationCourseTitle);
                    vm.Venues = VenueHelper.GetVenueNames(vm.Courses,_venueService).Result;
                    vm.Regions = allRegions;
                    break;
            }

            vm.NotificationTitle = notificationTitle;
            vm.CourseId = courseId;
            vm.CourseRunId = courseRunId;

            if (vm.AreAllReadyToBePublished)
            {
                if (publishMode == PublishMode.BulkUpload)
                    return RedirectToAction("PublishYourFile", "Bulkupload", new { NumberOfCourses = Courses.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).Count() });

            } else {
                if (publishMode == PublishMode.BulkUpload)
                {
                    var message = "";
                    if (fromBulkUpload)
                    {
                        var invalidCourseCount = Courses.Where(x => x.IsValid == false).Count();
                        var bulkUploadedPendingCourses = (Courses.SelectMany(c => c.CourseRuns)
                                           .Where(x => x.RecordStatus == RecordStatus.BulkUploadPending)
                                           .Count());
                        message = "Your file contained " + bulkUploadedPendingCourses + @WebHelper.GetErrorTextValueToUse(bulkUploadedPendingCourses) + ". You must fix all errors before your courses can be published to the directory.";
                        return RedirectToAction("WhatDoYouWantToDoNext", "Bulkupload", new { message = message });
                    }
                  
                   

                }
            }

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
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            else
                UKPRN = sUKPRN ?? 0;

            CompleteVM.NumberOfCoursesPublished = vm.NumberOfCoursesInFiles; 

            switch (vm.PublishMode)
            {
                case PublishMode.Migration:

                    // Publish migrated courses directly, NO archiving
                    var resultPublishMigratedCourses = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationReadyToGoLive, (int)RecordStatus.Live);
                    CompleteVM.Mode = PublishMode.Migration;
                    if (resultPublishMigratedCourses.IsSuccess)
                        return View("Complete", CompleteVM);
                    else
                        return RedirectToAction("Index", "Home", new { errmsg = "Publish All Migration-PublishCourses Error" });

                case PublishMode.BulkUpload:

                    await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationPending, (int)RecordStatus.Archived);
                    await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationReadyToGoLive, (int)RecordStatus.Archived);

                    //Archive any existing courses
                    var resultArchivingCourses = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.Live, (int)RecordStatus.Archived);
                    if (resultArchivingCourses.IsSuccess)
                    {
                        // Publish courses
                        var resultPublishBulkUploadedCourses = await _courseService.ChangeCourseRunStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);
                        CompleteVM.Mode = PublishMode.BulkUpload;
                        if (resultPublishBulkUploadedCourses.IsSuccess)
                            return View("Complete", CompleteVM);
                        else
                            return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-PublishCourses Error" });

                    } else {
                        return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-ArchiveCourses Error" });
                    }

                default:
                    return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload/Migration Error" });
            }            
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid courseId, Guid courseRunId,string courseName)
        {
            string notificationTitle = string.Empty;
            var result = await _courseService.UpdateStatus(courseId, courseRunId, (int)RecordStatus.Deleted);

            if (result.IsSuccess)
                notificationTitle = courseName + " was successfully deleted";
            else
                notificationTitle = "Error " + courseName + " was not deleted";

            return RedirectToAction("Index", "PublishCourses", new { publishMode = PublishMode.Migration,notificationTitle = notificationTitle });
        }

        public bool CheckAreAllReadyToBePublished(List<Course> courses, PublishMode publishMode)
        {
            bool AreAllReadyToBePublished = false;

            if(courses.Count.Equals(0))
                return AreAllReadyToBePublished;

            var hasInvalidCourses = courses.Any(c => c.IsValid == false);
            if (hasInvalidCourses)
                return AreAllReadyToBePublished;
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
        public async Task<IActionResult> DownloadErrorFile()
        {
            var model = new DownloadErrorFileViewModel();
            model.ErrorFileCreatedDate = DateTime.Now;
            return View("../DownloadErrorFile/Index", model);
        }


        internal IEnumerable<Course> GetErrorMessages(IEnumerable<Course> courses, ValidationMode validationMode)
        {
            foreach (var course in courses)
            {
                bool saveMe = false;
                course.ValidationErrors = _courseService.ValidateCourse(course).Select(x => x.Value);
                if (validationMode == ValidationMode.BulkUploadCourse && course.BulkUploadErrors.Any() && !course.ValidationErrors.Any()) {
                    course.BulkUploadErrors = new BulkUploadError[] { };
                    saveMe = true;
                }
                foreach (var courseRun in course.CourseRuns)
                {
                    courseRun.ValidationErrors = _courseService.ValidateCourseRun(courseRun, validationMode).Select(x => x.Value);
                    if (validationMode == ValidationMode.BulkUploadCourse && courseRun.BulkUploadErrors.Any() && !courseRun.ValidationErrors.Any())
                        courseRun.BulkUploadErrors = new BulkUploadError[] { };
                }

                // Save bulk upload fixed courses so that DQI stats will reflect new error counts
                if (validationMode == ValidationMode.BulkUploadCourse && saveMe)
                    _courseService.UpdateCourseAsync(course);
            }
            return courses;
        }
    }
}
