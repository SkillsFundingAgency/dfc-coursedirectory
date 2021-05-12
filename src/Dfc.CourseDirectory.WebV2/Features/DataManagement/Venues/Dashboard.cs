using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.Dashboard
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public bool ShowApprenticeships { get; set; }
        public bool ShowCourses { get; set; }
        public int PublishedApprenticeshipsCount { get; set; }
        public int PublishedCourseCount { get; set; }
        public int PublishedVenueCount { get; set; }
        public bool VenueUploadInProgress { get; set; }
        public int UnpublishedVenueCount { get; set; }
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
                new GetLatestVenueUploadForProviderWithStatus()
                {
                    ProviderId = providerContext.ProviderInfo.ProviderId,
                    Statuses = new[] { UploadStatus.Created, UploadStatus.Processing }
                });

            return new ViewModel()
            {
                PublishedApprenticeshipsCount = counts.ApprenticeshipCounts.GetValueOrDefault(ApprenticeshipStatus.Live, 0),
                PublishedCourseCount = counts.CourseRunCounts.GetValueOrDefault(CourseStatus.Live, 0),
                PublishedVenueCount = counts.VenueCount,
                ShowApprenticeships = providerType.HasFlag(ProviderType.Apprenticeships),
                ShowCourses = providerType.HasFlag(ProviderType.FE),
                VenueUploadInProgress = venueUploadStatus != null && venueUploadStatus.UploadStatus == UploadStatus.Processing,
                UnpublishedVenueCount = counts.UnpublishedVenueCount
            };
        }
    }
}
