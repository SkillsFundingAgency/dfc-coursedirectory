using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Dashboard
{
    public class Dashboard : ViewComponent
    {
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public Dashboard(ICourseService courseService, IVenueService venueService, IHttpContextAccessor contextAccessor, IBlobStorageService blobStorageService)
        {
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(blobStorageService, nameof(blobStorageService));

            _courseService = courseService;
            _venueService = venueService;
            _contextAccessor = contextAccessor;
            _blobStorageService = blobStorageService;
        }

        public async Task<IViewComponentResult> InvokeAsync(DashboardModel model)
        {
            var actualModel = model ?? new DashboardModel();

            int UKPRN = 0;
            if (_session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }

            var allVenues = await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), ""));
            
            IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                               .Result
                                               .Value
                                               .Value
                                               .SelectMany(o => o.Value)
                                               .SelectMany(i => i.Value);

            IEnumerable<CourseRun> bulkUploadReadyToGoLive = courses.SelectMany(c => c.CourseRuns)
                                                                       .Where(x => x.RecordStatus == RecordStatus.BulkUploadReadyToGoLive);

            IEnumerable<Course> validCourses = courses.Where(c => c.IsValid);

            IEnumerable<CourseValidationResult> results = _courseService.CourseValidationMessages(validCourses.Where(x => ((int)x.CourseStatus & (int)RecordStatus.Live) > 0),ValidationMode.DataQualityIndicator).Value;

            IEnumerable<string> courseMessages = results.SelectMany(c => c.Issues);
            IEnumerable<string> runMessages = results.SelectMany(c => c.RunValidationResults).SelectMany(r => r.Issues);
            IEnumerable<string> messages = courseMessages.Concat(runMessages)
                                                                 .GroupBy(i => i)
                                                                 .Select(g => $"{ g.LongCount() } { g.Key }");

            IEnumerable<Course> bulkUploadCoursesPending = courses.Where(x => ((int)x.CourseStatus & (int)RecordStatus.BulkUploadPending) > 0);
            IEnumerable<CourseRun> bulkUploadRunsPending = courses.SelectMany(c => c.CourseRuns)
                                                                    .Where(x => x.RecordStatus == RecordStatus.BulkUploadPending);


            IEnumerable<CourseRun> migrationPendingCourses = courses.SelectMany(c => c.CourseRuns).Where(x => x.RecordStatus == RecordStatus.MigrationPending);

            IEnumerable<Course> inValidCourses = courses.Where(c => c.IsValid == false);

            actualModel.DisplayMigrationButton = false;
            if (inValidCourses.Count() > 0 || migrationPendingCourses.Count() > 0)
            {
                actualModel.DisplayMigrationButton = true;
            }

            actualModel.BulkUploadPendingCount = bulkUploadRunsPending.Count();
            actualModel.BulkUploadReadyToGoLiveCount = bulkUploadReadyToGoLive.Count();
            actualModel.BulkUploadTotalCount = bulkUploadCoursesPending.Count() + bulkUploadReadyToGoLive.Count();

            IEnumerable<Services.BlobStorageService.BlobFileInfo> list = _blobStorageService.GetFileList(UKPRN + "/Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
            if (list.Any())
                actualModel.FileUploadDate = list.FirstOrDefault().DateUploaded.Value;

            string BulkUpLoadErrorMessage = actualModel.BulkUploadPendingCount.ToString() + WebHelper.GetCourseTextToUse(actualModel.BulkUploadTotalCount) + " upload in a file on "
                                                    + actualModel.FileUploadDate?.ToString("dd/MM/yyyy") + " have "
                                                    + (bulkUploadCoursesPending?.SelectMany(c => c.BulkUploadErrors).Count() + bulkUploadRunsPending?.SelectMany(r => r.BulkUploadErrors).Count()).ToString()
                                                    + " errors. Fix these to publish all of your courses.";

            string BulkUpLoadNoErrorMessage = actualModel.BulkUploadTotalCount.ToString() + WebHelper.GetCourseTextToUse(actualModel.BulkUploadPendingCount) + " uploaded on " + actualModel.FileUploadDate?.ToString("dd/MM/yyyy") + " have no errors, but are not listed on the Course directory becuase you have not published them.";
            actualModel.FileCount = list.Count();

            int MigrationLiveCount = courses.Where(x => x.CourseStatus == RecordStatus.Live && x.CreatedBy == "DFC – Course Migration Tool")
                                            .SelectMany(c => c.CourseRuns)
                                            .Where(x => x.RecordStatus == RecordStatus.Live && x.CreatedBy == "DFC – Course Migration Tool")
                                            .Count();

            actualModel.BulkUploadMessage = (actualModel.BulkUploadTotalCount > 0 & actualModel.BulkUploadPendingCount == 0) ? BulkUpLoadNoErrorMessage : BulkUpLoadErrorMessage;

            actualModel.ValidationMessages = messages;
            actualModel.VenueCount = 0;
            if (allVenues != null)
            {
                actualModel.VenueCount = allVenues.Value.Value.Count();
            }

            actualModel.PublishedCourseCount = courses.Where(x => x.CourseStatus == RecordStatus.Live)
                                                 .SelectMany(c => c.CourseRuns)
                                                 .Where(x => x.RecordStatus == RecordStatus.Live)
                                                 .Count();

            return View("~/ViewComponents/Dashboard/Default.cshtml", actualModel);
        }
    }
}
