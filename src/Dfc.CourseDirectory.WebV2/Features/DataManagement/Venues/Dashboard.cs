using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Extensions;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.Dashboard
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public bool ShowCourses { get; set; }
        public bool ShowNonLars { get; set; }
        public int PublishedCourseCount { get; set; }
        public int PublishedNonLarsCourseCount { get; set; }
        public bool CourseUploadInProgress { get; set; }
        public bool NonLarsUploadInProgress { get; set; }
        public int PublishedVenueCount { get; set; }
        public bool VenueUploadInProgress { get; set; }
        public int UnpublishedVenueCount { get; set; }
        public int UnpublishedCourseCount { get; set; }
        public int UnpublishedNonLarsCourseCount { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerContext = _providerContextProvider.GetProviderContext();

            var counts = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderDashboardCounts()
            {
                ProviderId = providerContext.ProviderInfo.ProviderId,
                Date = _clock.UtcNow.Date
            });

            var providerType = providerContext.ProviderInfo.ProviderType;

            var venueUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUnpublishedVenueUploadForProvider()
                {
                    ProviderId = providerContext.ProviderInfo.ProviderId
                });

            var courseUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerContext.ProviderInfo.ProviderId,
                    IsNonLars = false
                });

            var nonLarsUploadStatus = await _sqlQueryDispatcher.ExecuteQuery(
               new GetLatestUnpublishedCourseUploadForProvider()
               {
                   ProviderId = providerContext.ProviderInfo.ProviderId,
                   IsNonLars = true
               });
            return new ViewModel()
            {
                PublishedCourseCount = counts.CourseRunCount,
                PublishedNonLarsCourseCount = counts.NonLarsCourseCount,
                PublishedVenueCount = counts.VenueCount,
                ShowCourses = providerType.HasFlag(ProviderType.FE),
                ShowNonLars = providerType.HasFlag(ProviderType.NonLARS),
                VenueUploadInProgress = venueUploadStatus != null && (venueUploadStatus.UploadStatus == UploadStatus.Processing || venueUploadStatus.UploadStatus == UploadStatus.Created),
                UnpublishedVenueCount = counts.UnpublishedVenueCount,
                UnpublishedCourseCount = counts.UnpublishedCourseCount,
                UnpublishedNonLarsCourseCount = counts.UnpublishedNonLarsCourseCount,
                CourseUploadInProgress = courseUploadStatus != null && (courseUploadStatus.UploadStatus == UploadStatus.Processing || courseUploadStatus.UploadStatus == UploadStatus.Created),
                NonLarsUploadInProgress = nonLarsUploadStatus != null && (nonLarsUploadStatus.UploadStatus == UploadStatus.Processing || nonLarsUploadStatus.UploadStatus == UploadStatus.Created),
            };
        }
    }
}
