using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.ViewModels.ProviderDashboard
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
        public bool ShowNonLars { get; set; }
        public bool ShowTLevels { get; set; }
        public int CourseRunCount { get; set; }
        public int NonLarsCourseCount { get; set; }
        public int PastStartDateCourseRunCount { get; set; }
        public int TLevelCount { get; set; }
        public int VenueCount { get; set; }
        public bool IsNewProvider { get; set; }
        public bool VenueUploadInProgress { get; set; }
        public int UnpublishedVenueCount { get; set; }
        public int UnpublishedCourseCount { get; set; }
        public int UnpublishedNonLarsCourseCount { get; set; }
        public bool CourseUploadInProgress { get; set; }
        public bool NonLarsCourseUploadInProgress { get; set; }
        public int PastStartDateNonLarsCourseRunCount { get; set; }
        public int PastStartDateTLevelRunCount { get; set; }
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
            var provider = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderById() { ProviderId = request.ProviderId });

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
                    ProviderId = provider.ProviderId,
                    IsNonLars = false
                });

            var nonlarsUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = provider.ProviderId,
                    IsNonLars = true
                });

            var vm = new ViewModel()
            {
                ProviderName = provider.ProviderName,
                Ukprn = provider.Ukprn,
                ShowCourses = provider.ProviderType.HasFlag(ProviderType.FE),
                ShowNonLars = provider.ProviderType.HasFlag(ProviderType.NonLARS),
                ShowTLevels = provider.ProviderType.HasFlag(ProviderType.TLevels),
                CourseRunCount = dashboardCounts.CourseRunCount,
                NonLarsCourseCount = dashboardCounts.NonLarsCourseCount,
                PastStartDateCourseRunCount = dashboardCounts.PastStartDateCourseRunCount,
                PastStartDateNonLarsCourseRunCount = dashboardCounts.PastStartDateNonLarsCourseRunCount,
                PastStartDateTLevelRunCount = dashboardCounts.PastStartDateTLevelRunCount,
                TLevelCount = dashboardCounts.TLevelCount,
                VenueCount = dashboardCounts.VenueCount,
                IsNewProvider = provider.ProviderType == ProviderType.None,
                VenueUploadInProgress = venueUploadStatus != null && (venueUploadStatus.UploadStatus == UploadStatus.Processing || venueUploadStatus.UploadStatus == UploadStatus.Created),
                UnpublishedVenueCount = dashboardCounts.UnpublishedVenueCount,
                UnpublishedCourseCount = dashboardCounts.UnpublishedCourseCount,
                UnpublishedNonLarsCourseCount = dashboardCounts.UnpublishedNonLarsCourseCount,
                CourseUploadInProgress = courseUploadStatus != null && (courseUploadStatus.UploadStatus == UploadStatus.Processing || courseUploadStatus.UploadStatus == UploadStatus.Created),
                NonLarsCourseUploadInProgress = nonlarsUploadStatus != null && (nonlarsUploadStatus.UploadStatus == UploadStatus.Processing || nonlarsUploadStatus.UploadStatus == UploadStatus.Created)
            };

            return vm;
        }
    }
}
