using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

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
        public int CourseRunCount { get; set; }
        public int PastStartDateCourseRunCount { get; set; }
        public int ApprenticeshipCount { get; set; }
        public int TLevelCount { get; set; }
        public int VenueCount { get; set; }
        public bool IsNewProvider { get; set; }
        public bool VenueUploadInProgress { get; set; }
        public int UnpublishedVenueCount { get; set; }
        public int UnpublishedCourseCount { get; set; }
        public bool CourseUploadInProgress { get; set; }
        public bool ApprenticeshipUploadInProgress { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher, IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
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

            var venueUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUnpublishedVenueUploadForProvider()
                {
                    ProviderId = request.ProviderId
                });

            var courseUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = provider.ProviderId
                });

            var apprenticeshipUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUnpublishedApprenticeshipUploadForProvider()
                {
                    ProviderId = provider.ProviderId
                });

            var vm = new ViewModel()
            {
                ProviderName = provider.ProviderName,
                Ukprn = provider.Ukprn,
                ShowCourses = provider.ProviderType.HasFlag(ProviderType.FE),
                ShowApprenticeships = provider.ProviderType.HasFlag(ProviderType.Apprenticeships) && provider.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Passed,
                ShowTLevels = provider.ProviderType.HasFlag(ProviderType.TLevels),
                CourseRunCount = dashboardCounts.CourseRunCount,
                PastStartDateCourseRunCount = dashboardCounts.PastStartDateCourseRunCount,
                ApprenticeshipCount = dashboardCounts.ApprenticeshipCounts.GetValueOrDefault(ApprenticeshipStatus.Live),
                TLevelCount = dashboardCounts.TLevelCount,
                VenueCount = dashboardCounts.VenueCount,
                IsNewProvider = provider.ProviderType == ProviderType.None,
                VenueUploadInProgress = venueUploadStatus != null && (venueUploadStatus.UploadStatus == UploadStatus.Processing || venueUploadStatus.UploadStatus == UploadStatus.Created),
                UnpublishedVenueCount = dashboardCounts.UnpublishedVenueCount,
                UnpublishedCourseCount = dashboardCounts.UnpublishedCourseCount,
                CourseUploadInProgress = courseUploadStatus != null && (courseUploadStatus.UploadStatus == UploadStatus.Processing || courseUploadStatus.UploadStatus == UploadStatus.Created),
                ApprenticeshipUploadInProgress = apprenticeshipUploadStatus != null && (apprenticeshipUploadStatus.UploadStatus == UploadStatus.Processing || apprenticeshipUploadStatus.UploadStatus == UploadStatus.Created)
            };

            return vm;
        }
    }
}
