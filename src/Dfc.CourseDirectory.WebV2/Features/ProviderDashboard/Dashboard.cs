using System;
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
        public int CourseCount { get; set; }
        public int ApprenticeshipCount { get; set; }
        public int VenueCount { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderById() { ProviderId = request.ProviderId });

            if (provider == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, request.ProviderId);
            }

            var (courseCount, apprenticeshipCount, venueCount) = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderDashboardCounts() { ProviderId = request.ProviderId });

            var vm = new ViewModel()
            {
                ApprenticeshipCount = apprenticeshipCount,
                CourseCount = courseCount,
                VenueCount = venueCount,
                ShowApprenticeships = provider.ProviderType.HasFlag(ProviderType.Apprenticeships) &&
                    provider.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Passed,
                ShowCourses = provider.ProviderType.HasFlag(ProviderType.FE),
                ProviderName = provider.ProviderName,
                Ukprn = provider.Ukprn
            };

            return vm;
        }
    }
}
