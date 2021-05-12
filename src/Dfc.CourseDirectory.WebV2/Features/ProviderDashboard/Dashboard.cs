using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using CosmosDbQueries = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Features.ProviderDashboard.Dashboard
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public string ProviderName { get; set; }
        public int Ukprn { get; set; }
        public bool ShowCourses { get; set; }
        public bool ShowApprenticeships { get; set; }
        public bool ShowTLevels { get; set; }
        public int LiveCourseRunCount { get; set; }
        public int PastStartDateCourseRunCount { get; set; }
        public int MigrationPendingCourseRunCount { get; set; }
        public int BulkUploadPendingCourseRunCount { get; set; }
        public int BulkUploadReadyToGoLiveCourseRunCount { get; set; }
        public int BulkUploadCoursesErrorCount { get; set; }
        public int BulkUploadCourseRunsErrorCount { get; set; }
        public int LarslessCourseCount { get; set; }
        public int ApprenticeshipCount { get; set; }
        public int BulkUploadPendingApprenticeshipsCount { get; set; }
        public int BulkUploadReadyToGoLiveApprenticeshipsCount { get; set; }
        public int ApprenticeshipsBulkUploadErrorCount { get; set; }
        public int TLevelCount { get; set; }
        public int VenueCount { get; set; }
        public int BulkUploadFileCount { get; set; }
        public bool BulkUploadInProgress { get; set; }
        public bool IsNewProvider { get; set; }
        public bool VenueUploadInProgress { get; set; }
        public int UnpublishedVenueCount { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IBinaryStorageProvider _binaryStorageProvider;
        private readonly IClock _clock;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher, ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IBinaryStorageProvider binaryStorageProvider, IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _binaryStorageProvider = binaryStorageProvider;
            _clock = clock;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderById() { ProviderId = request.ProviderId });

            if (provider == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, request.ProviderId);
            }

            var dashboardCounts = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderDashboardCounts
                {
                    ProviderId = request.ProviderId,
                    Date = _clock.UtcNow.ToLocalTime().Date
                });

            var courseMigrationReport = await _cosmosDbQueryDispatcher.ExecuteQuery(new CosmosDbQueries.GetCourseMigrationReportForProvider { ProviderUkprn = provider.Ukprn });

            var bulkUploadFiles = await _binaryStorageProvider.ListFiles($"{provider.Ukprn}/Bulk Upload/Files/");

            var venueUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUnpublishedVenueUploadForProvider()
                {
                    ProviderId = request.ProviderId
                });

            var vm = new ViewModel()
            {
                ProviderName = provider.ProviderName,
                Ukprn = provider.Ukprn,
                ShowCourses = provider.ProviderType.HasFlag(ProviderType.FE),
                ShowApprenticeships = provider.ProviderType.HasFlag(ProviderType.Apprenticeships) && provider.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Passed,
                ShowTLevels = provider.ProviderType.HasFlag(ProviderType.TLevels),
                LiveCourseRunCount = dashboardCounts.CourseRunCounts.GetValueOrDefault(CourseStatus.Live),
                PastStartDateCourseRunCount = dashboardCounts.PastStartDateCourseRunCount,
                MigrationPendingCourseRunCount = dashboardCounts.CourseRunCounts.GetValueOrDefault(CourseStatus.MigrationPending) + dashboardCounts.CourseRunCounts.GetValueOrDefault(CourseStatus.MigrationReadyToGoLive),
                BulkUploadPendingCourseRunCount = dashboardCounts.CourseRunCounts.GetValueOrDefault(CourseStatus.BulkUploadPending),
                BulkUploadReadyToGoLiveCourseRunCount = dashboardCounts.CourseRunCounts.GetValueOrDefault(CourseStatus.BulkUploadReadyToGoLive),
                BulkUploadCoursesErrorCount = dashboardCounts.BulkUploadCoursesErrorCount,
                BulkUploadCourseRunsErrorCount = dashboardCounts.BulkUploadCourseRunsErrorCount,
                LarslessCourseCount = courseMigrationReport?.LarslessCourses?.Count ?? 0,
                ApprenticeshipCount = dashboardCounts.ApprenticeshipCounts.GetValueOrDefault(ApprenticeshipStatus.Live),
                BulkUploadPendingApprenticeshipsCount = dashboardCounts.ApprenticeshipCounts.GetValueOrDefault(ApprenticeshipStatus.BulkUploadPending),
                BulkUploadReadyToGoLiveApprenticeshipsCount = dashboardCounts.ApprenticeshipCounts.GetValueOrDefault(ApprenticeshipStatus.BulkUploadReadyToGoLive),
                ApprenticeshipsBulkUploadErrorCount = dashboardCounts.ApprenticeshipsBulkUploadErrorCount,
                TLevelCount = dashboardCounts.TLevelCounts.GetValueOrDefault(TLevelStatus.Live),
                VenueCount = dashboardCounts.VenueCount,
                BulkUploadFileCount = bulkUploadFiles.Count(),
                BulkUploadInProgress = provider.BulkUploadInProgress ?? false,
                IsNewProvider = provider.ProviderType == ProviderType.None,
                VenueUploadInProgress = venueUploadStatus != null && venueUploadStatus.UploadStatus == UploadStatus.InProgress,
                UnpublishedVenueCount = dashboardCounts.UnpublishedVenueCount
            };

            return vm;
        }
    }
}
