using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;
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
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Web.ViewModels.Migration;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;

namespace Dfc.CourseDirectory.Web.ViewComponents.Dashboard
{
    public class Dashboard : ViewComponent
    {
        private readonly ICourseService _courseService;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IVenueService _venueService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IProviderService _providerService;
        private readonly IEnvironmentHelper _environmentHelper;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IFeatureFlagProvider _featureFlagProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Dashboard(ICourseService courseService, IVenueService venueService, IHttpContextAccessor contextAccessor, IBlobStorageService blobStorageService, IApprenticeshipService apprenticeshipService, IProviderService providerService,
            IEnvironmentHelper environmentHelper, IFeatureFlagProvider featureFlagProvider, ISqlQueryDispatcher sqlQueryDispatcher)
        {
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(blobStorageService, nameof(blobStorageService));
            Throw.IfNull(providerService, nameof(providerService));
            Throw.IfNull(environmentHelper, nameof(environmentHelper));
       

            _apprenticeshipService = apprenticeshipService;
            _courseService = courseService;
            _venueService = venueService;
            _contextAccessor = contextAccessor;
            _blobStorageService = blobStorageService;
            _providerService = providerService;
            _environmentHelper = environmentHelper;
            _featureFlagProvider = featureFlagProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
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

            try
            {
                var getCoursesResult = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN)).Result;
                IEnumerable<Course> courses = getCoursesResult
                                                   .Value
                                                   .Value
                                                   .SelectMany(o => o.Value)
                                                   .SelectMany(i => i.Value);

                IEnumerable<CourseRun> bulkUploadReadyToGoLive = courses.SelectMany(c => c.CourseRuns)
                                                                           .Where(x => x.RecordStatus == RecordStatus.BulkUploadReadyToGoLive);

                IEnumerable<Course> validCourses = courses.Where(c => c.IsValid);

                IEnumerable<CourseValidationResult> results = _courseService.CourseValidationMessages(validCourses.Where(x => ((int)x.CourseStatus & (int)RecordStatus.Live) > 0), ValidationMode.DataQualityIndicator).Value;

                IEnumerable<string> courseMessages = results.SelectMany(c => c.Issues);
                IEnumerable<string> runMessages = results.SelectMany(c => c.RunValidationResults).SelectMany(r => r.Issues);
                IEnumerable<string> messages = courseMessages.Concat(runMessages)
                                                                     .GroupBy(i => i)
                                                                     .Select(g => $"{ g.LongCount() } { g.Key }");

                IEnumerable<Course> bulkUploadCoursesPending = courses.Where(x => ((int)x.CourseStatus & (int)RecordStatus.BulkUploadPending) > 0);
                IEnumerable<CourseRun> bulkUploadRunsPending = courses.SelectMany(c => c.CourseRuns)
                                                                        .Where(x => x.RecordStatus == RecordStatus.BulkUploadPending);


                IEnumerable<CourseRun> migrationPendingCourses = courses.SelectMany(c => c.CourseRuns).Where(x => x.RecordStatus == RecordStatus.MigrationPending || x.RecordStatus == RecordStatus.MigrationReadyToGoLive);

                IEnumerable<Course> inValidCourses = courses.Where(c => c.IsValid == false);

                var getApprenticeshipResult = _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString()).Result;          

               

                var ApprenticeshipBulkUploadReadyToGoLive = _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString()).Result.Value.Where(x => x.RecordStatus == RecordStatus.BulkUploadReadyToGoLive);

               actualModel.ApprenticeshipBulkUploadReadyToGoLiveCount = ApprenticeshipBulkUploadReadyToGoLive.Count();

                actualModel.BulkUploadPendingCount = bulkUploadRunsPending.Count();
                actualModel.BulkUploadReadyToGoLiveCount = bulkUploadReadyToGoLive.Count();
                actualModel.BulkUploadTotalCount = bulkUploadCoursesPending.Count() + bulkUploadReadyToGoLive.Count();


                IEnumerable<Services.BlobStorageService.BlobFileInfo> list = _blobStorageService.GetFileList(UKPRN + "/Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
                if (list.Any())
                    actualModel.FileUploadDate = list.FirstOrDefault().DateUploaded.Value;

               var courseMigrationReportResult = await _courseService.GetCourseMigrationReport(UKPRN);

               var larslessCoursesCount = courseMigrationReportResult?.Value==null?0:courseMigrationReportResult?.Value.LarslessCourses.Count();
                
                actualModel.DisplayMigrationButton = false;
                //list.Any() to see if any bulkupload files exist. If they do we don't want to show migration error.
                if ((migrationPendingCourses.Count() > 0 || larslessCoursesCount>0) && !list.Any()) 
                {
                    actualModel.DisplayMigrationButton = true;
                }

                actualModel.BulkUpLoadHasErrors = bulkUploadCoursesPending?.SelectMany(c => c.BulkUploadErrors).Count() + bulkUploadRunsPending?.SelectMany(r => r.BulkUploadErrors).Count() > 0;

                string BulkUpLoadErrorMessage = actualModel.BulkUploadTotalCount.ToString() + WebHelper.GetCourseTextToUse(actualModel.BulkUploadTotalCount) + " uploaded in a file on "
                                                        + actualModel.FileUploadDate?.ToString("dd/MM/yyyy") + " have "
                                                        + (bulkUploadCoursesPending?.SelectMany(c => c.BulkUploadErrors).Count() + bulkUploadRunsPending?.SelectMany(r => r.BulkUploadErrors).Count()).ToString()
                                                        + " errors. Fix these to publish all of your courses.";

                string BulkUpLoadNoErrorMessage = "Your bulk upload is complete." + actualModel.BulkUploadTotalCount.ToString() + WebHelper.GetCourseTextToUse(actualModel.BulkUploadPendingCount) + " have been uploaded on " + actualModel.FileUploadDate?.ToString("dd/MM/yyyy") + " and ready to publish to the course directory.";
                actualModel.FileCount = list.Count();

                int MigrationLiveCount = courses.Where(x => x.CourseStatus == RecordStatus.Live && x.CreatedBy == "DFC – Course Migration Tool")
                                                .SelectMany(c => c.CourseRuns)
                                                .Count(x => x.RecordStatus == RecordStatus.Live && x.CreatedBy == "DFC – Course Migration Tool");

                actualModel.BulkUploadMessage = (actualModel.BulkUploadTotalCount > 0 & actualModel.BulkUploadPendingCount == 0) ? BulkUpLoadNoErrorMessage : BulkUpLoadErrorMessage;


                actualModel.ValidationMessages = messages;
                actualModel.VenueCount = 0;
                if (allVenues.Value != null)
                {
                    actualModel.VenueCount = allVenues.Value.Value.Count(x => x.Status == VenueStatus.Live);
                }

                actualModel.PublishedCourseCount = courses
                                                  .SelectMany(c => c.CourseRuns)
                                                  .Count(x => x.RecordStatus == RecordStatus.Live);

                var result = await _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString());

                var appResult = await _apprenticeshipService.GetApprenticeshipDashboardCounts(UKPRN);

                if (appResult.IsSuccess && appResult.HasValue)
                {
                    var counts = appResult.Value;
                    IEnumerable<Services.BlobStorageService.BlobFileInfo> appList = _blobStorageService.GetFileList(UKPRN + "/Apprenticeship Bulk Upload/Files/").OrderByDescending(x => x.DateUploaded).ToList();
                    if (list.Any())
                        counts.FileUploadDate = list.FirstOrDefault().DateUploaded.Value;

                    var appMessages = GenerateApprenticeshipDQIMessages(counts);

                    if (!string.IsNullOrWhiteSpace(appMessages))
                    {
                        actualModel.ApprenticeshipMessages = appMessages;
                        actualModel.ApprenticeshipHasErrors = true;                        
                        
                    }

                    if(counts.TotalErrors != null && counts.TotalErrors > 0 )
                    {
                        actualModel.ApprenticeshipBulkUploadHasErrors = true;
                    }
                    else
                    {
                        actualModel.ApprenticeshipBulkUploadHasErrors = false;
                    }
                }
                // provider has no apprenticeship but pending bulkupload 
                if(appResult.IsFailure)
                {
                    var counts = appResult.Value;                 

                    if (counts == null && actualModel.ApprenticeshipBulkUploadReadyToGoLiveCount > 0)
                    {
                        //var appMessages =  actualModel.ApprenticeshipBulkUploadReadyToGoLiveCount.ToString() + WebHelper.GetCourseTextToUse(actualModel.ApprenticeshipBulkUploadReadyToGoLiveCount) + " uploaded on " + actualModel.FileUploadDate?.ToString("dd/MM/yyyy") + " have no errors, but are not listed on the Course directory because you have not published them.";
                        var appMessages = "Your bulk upload is complete." + actualModel.ApprenticeshipBulkUploadReadyToGoLiveCount.ToString() + WebHelper.GetApprenticeshipsTextToUse(actualModel.ApprenticeshipBulkUploadReadyToGoLiveCount) + " have been uploaded on " + actualModel.FileUploadDate?.ToString("dd/MM/yyyy") + "and ready to publish to the course directory.";
                        if (!string.IsNullOrWhiteSpace(appMessages))
                        {
                            actualModel.ApprenticeshipMessages = appMessages;
                        }                     
                    }
                }

           
                actualModel.PublishedApprenticeshipsCount = result.Value.Count(x => x.RecordStatus == RecordStatus.Live);

            Dfc.CourseDirectory.Models.Models.Providers.Provider provider = FindProvider(UKPRN);
            if (null != provider)
            {
                if(null != provider.BulkUploadStatus)
                {
                    actualModel.BulkUploadBackgroundInProgress = provider.BulkUploadStatus.InProgress;
                    actualModel.BulkUploadBackgroundRowCount = provider.BulkUploadStatus.TotalRowCount;
                    actualModel.BulkUploadBackgroundStartTimestamp = provider.BulkUploadStatus.StartedTimestamp;
                        actualModel.BulkUploadPublishInProgress = provider.BulkUploadStatus.PublishInProgress;
                }
                actualModel.ProviderType = provider.ProviderType;
            }
            actualModel.EnvironmentType = _environmentHelper.GetEnvironmentType();

                actualModel.QAFeatureIsEnabled = _featureFlagProvider.HaveFeature(FeatureFlags.ApprenticeshipQA);

                if (actualModel.QAFeatureIsEnabled)
                {
                    var providerId = ViewContext.HttpContext.Features.Get<ProviderContextFeature>().ProviderInfo.ProviderId;

                    actualModel.ProviderQACurrentStatus = await _sqlQueryDispatcher.ExecuteQuery(
                        new GetProviderApprenticeshipQAStatus()
                        {
                            ProviderId = providerId
                        }) ?? WebV2.Models.ApprenticeshipQAStatus.NotStarted;

                }

            }
            catch (Exception ex)
            {
                //@ToDo: decide how to handle this - should at least be logged. Caused by NPE during call to course service
                List<string> errors = new List<string>() { "There was a system problem whilst obtaining course data from the course directory. Please wait a few moments and refresh your browser page." };
                actualModel.ValidationMessages = errors;
            }
            return View("~/ViewComponents/Dashboard/Default.cshtml", actualModel);
        }

        private Dfc.CourseDirectory.Models.Models.Providers.Provider FindProvider(int prn)
        {
            Dfc.CourseDirectory.Models.Models.Providers.Provider provider = null;
            try
            {
                var providerSearchResult = Task.Run(async () => await _providerService.GetProviderByPRNAsync(new Services.ProviderService.ProviderSearchCriteria(prn.ToString()))).Result;
                if (providerSearchResult.IsSuccess)
                {
                    provider = providerSearchResult.Value.Value.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                // @ToDo: decide how to handle this
            }
            return provider;
        }

        private string GenerateApprenticeshipDQIMessages(ApprenticeshipDashboardCounts counts)
        {
            var messages = new List<string>();
            
            int totalAppCount = 0;
            if (counts.BulkUploadPendingCount.HasValue) totalAppCount += counts.BulkUploadPendingCount.Value;
            if (counts.BulkUploadReadyToGoLiveCount.HasValue) totalAppCount += counts.BulkUploadReadyToGoLiveCount.Value;

            if (counts.TotalErrors.HasValue && counts.TotalErrors.Value > 0)
            {
                return $"{totalAppCount} apprenticeship{(totalAppCount > 1 ? "s" : "")} uploaded in a file on {counts.FileUploadDate.Value:dd/MM/yyyy} {(totalAppCount > 1 ? "have" : "has")} {counts.TotalErrors} errors. " +
                             $"Fix these to publish all of your apprenticeship{(totalAppCount > 1 ? "s" : "")}.";
                
            }

            if (counts.BulkUploadReadyToGoLiveCount.HasValue && counts.BulkUploadReadyToGoLiveCount.Value > 0)
            {
                if (counts.BulkUploadPendingCount.HasValue && counts.BulkUploadPendingCount.Value == 0)
                {
                    return $"{totalAppCount} apprenticeship{(totalAppCount > 1 ? "s" : "")} uploaded in a file on {counts.FileUploadDate.Value:dd/MM/yyyy} {(totalAppCount > 1 ? "have" : "has")} no errors but are not listed on the Course directory because you have not published {(totalAppCount > 1 ? "them" : "it")}.";
                }
            }
            return string.Empty;
        }
    }
}
