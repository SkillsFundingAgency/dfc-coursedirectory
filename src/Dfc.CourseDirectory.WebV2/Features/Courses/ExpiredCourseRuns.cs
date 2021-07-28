using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.Courses.ExpiredCourseRuns
{
    public class Query : IRequest<ViewModel>
    {
    }
    
    public class ViewModel
    {
        public int Total { get; set; }
        public IReadOnlyCollection<ViewModelRow> Rows { get; set; }
    }

    public class ViewModelRow
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public string LearnAimRef { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string VenueName { get; set; }
        public bool? National { get; set; }
        public IReadOnlyCollection<string> SubRegionNames { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public string LearnAimRefTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRefTypeDesc { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            IRegionCache regionCache)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _regionCache = regionCache;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetExpiredCourseRunsForProvider()
            {
                ProviderId = _providerContextProvider.GetProviderId(),
                Today = _clock.UtcNow.Date
            });

            var allRegions = await _regionCache.GetAllRegions();
            var allSubRegions = allRegions.SelectMany(r => r.SubRegions).ToDictionary(sr => sr.Id, sr => sr);

            return new ViewModel()
            {
                Rows = results
                    .Select(r => new ViewModelRow()
                    {
                        CourseId = r.CourseId,
                        CourseRunId = r.CourseRunId,
                        CourseName = r.CourseName,
                        ProviderCourseRef = r.ProviderCourseId,
                        LearnAimRef = r.LearnAimRef,
                        DeliveryMode = r.DeliveryMode,
                        VenueName = r.VenueName,
                        National = r.National,
                        SubRegionNames = r.SubRegionIds.Select(id => allSubRegions[id].Name).ToArray(),
                        StudyMode = r.StudyMode,
                        LearnAimRefTitle = r.LearnAimRefTitle,
                        NotionalNVQLevelv2 = r.NotionalNVQLevelv2,
                        AwardOrgCode = r.AwardOrgCode,
                        LearnAimRefTypeDesc = r.LearnAimRefTypeDesc,
                        StartDate = r.StartDate
                    })
                    .ToArray(),
                Total = results.Count
            };
        }
    }
}
