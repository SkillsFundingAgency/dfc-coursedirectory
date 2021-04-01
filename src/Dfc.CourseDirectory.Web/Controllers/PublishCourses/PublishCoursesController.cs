using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.PublishCourses
{
    public class PublishCoursesController : Controller
    {
        private ISession Session => HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderContextProvider _providerContextProvider;

        public PublishCoursesController(
            ICourseService courseService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(PublishMode publishMode, string notificationTitle, Guid? courseId, Guid? courseRunId, bool fromBulkUpload)
        {
            int? UKPRN = Session.GetInt32("UKPRN");
            
            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var coursesByUKPRN = (await _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;

            // Only display courses that have Lars and Qualification titles
            var courses = coursesByUKPRN.Value
                .SelectMany(o => o.Value)
                .SelectMany(i => i.Value)
                .Where(c => !string.IsNullOrWhiteSpace(c.LearnAimRef) && !string.IsNullOrWhiteSpace(c.QualificationCourseTitle))
                .ToList();

            courses = GetErrorMessages(courses, ValidationMode.MigrateCourse).ToList();

            PublishViewModel vm = new PublishViewModel();

            switch (publishMode)
            {
                case PublishMode.Migration:
                   if (courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.MigrationPending || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive) && x.IsValid == false).Any())
                    {
                        vm.PublishMode = PublishMode.Migration;

                        var migratedCourses = courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.MigrationPending || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive));
                        var migratedCoursesWithErrors = GetErrorMessages(migratedCourses, ValidationMode.MigrateCourse).ToList();

                        vm.NumberOfCoursesInFiles = migratedCoursesWithErrors.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.MigrationPending
                                                                                                                    || cr.RecordStatus == RecordStatus.MigrationReadyToGoLive)).Count();

                        vm.Courses = migratedCoursesWithErrors.OrderBy(x => x.QualificationCourseTitle);
                        vm.AreAllReadyToBePublished = CheckAreAllReadyToBePublished(migratedCoursesWithErrors, PublishMode.Migration);
                        vm.Venues = VenueHelper.GetVenueNames(vm.Courses, _sqlQueryDispatcher).Result;
                        break;
                    }
                    else
                    {
                        return View("../Migration/Complete/index");
                    }

                case PublishMode.BulkUpload:

                    vm.PublishMode = PublishMode.BulkUpload;
                    var bulkUploadedCourses = courses.Where(x => x.CourseRuns.Any(cr => cr.RecordStatus == RecordStatus.BulkUploadPending || cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).ToList();
                    vm.NumberOfCoursesInFiles = bulkUploadedCourses.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.BulkUploadPending || cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).Count();
                    vm.Courses = bulkUploadedCourses.OrderBy(x => x.QualificationCourseTitle);
                    vm.AreAllReadyToBePublished = CheckAreAllReadyToBePublished(bulkUploadedCourses, PublishMode.BulkUpload);
                    vm.Courses = GetErrorMessages(vm.Courses, ValidationMode.BulkUploadCourse);
                    vm.Venues = VenueHelper.GetVenueNames(vm.Courses, _sqlQueryDispatcher).Result;
                    break;

                case PublishMode.DataQualityIndicator:

                    vm.PublishMode = PublishMode.DataQualityIndicator;
                    var validCourses = courses.Where(x => x.IsValid && ((int)x.CourseStatus & (int)RecordStatus.Live) > 0);
                    var results = _courseService.CourseValidationMessages(validCourses, ValidationMode.DataQualityIndicator).Value.ToList();
                    var invalidCoursesResult = results.Where(c => c.RunValidationResults.Any(cr => cr.Issues.Count() > 0));
                    var invalidCourses = invalidCoursesResult.Select(c => (Course)c.Course).ToList();
                    var invalidCourseRuns = invalidCourses.Select(cr => cr.CourseRuns.Where(x => x.StartDate < DateTime.Today));
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

                    if (invalidCourseRuns.Count() == 0 && courseId != null && courseRunId != null)
                    {
                        return BadRequest();
                    }

                    vm.NumberOfCoursesInFiles = invalidCourses.Count();
                    vm.Courses = filteredList.OrderBy(x => x.QualificationCourseTitle);
                    vm.Venues = VenueHelper.GetVenueNames(vm.Courses, _sqlQueryDispatcher).Result;
                    vm.Regions = allRegions;
                    break;
            }

            vm.NotificationTitle = notificationTitle;
            vm.CourseId = courseId;
            vm.CourseRunId = courseRunId;

            if (vm.AreAllReadyToBePublished)
            {
                if (publishMode == PublishMode.BulkUpload)
                    return RedirectToAction("CoursesPublishFile", "Bulkupload", new { NumberOfCourses = courses.SelectMany(s => s.CourseRuns.Where(cr => cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).Count() })
                        .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true));

            } else {
                if (publishMode == PublishMode.BulkUpload)
                {
                    var message = "";
                    if (fromBulkUpload)
                    {
                        var invalidCourseCount = courses.Where(x => x.IsValid == false).Count();
                        var bulkUploadedPendingCourses = (courses.SelectMany(c => c.CourseRuns)
                                           .Where(x => x.RecordStatus == RecordStatus.BulkUploadPending)
                                           .Count());
                        message = "Your file contained " + bulkUploadedPendingCourses + @WebHelper.GetErrorTextValueToUse(bulkUploadedPendingCourses) + ". You must resolve all errors before your courses information can be published.";
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

            int? sUKPRN = Session.GetInt32("UKPRN");
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
        public IActionResult DownloadErrorFile()
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

                var crErrors = course.CourseRuns.Select(cr => cr.ValidationErrors.Count()).Sum();
                var cErrors = course.ValidationErrors.Count();

                if(crErrors > 0 || cErrors > 0)
                {
                    course.IsValid = false;
                }
            }
            return courses;
        }
    }
}
