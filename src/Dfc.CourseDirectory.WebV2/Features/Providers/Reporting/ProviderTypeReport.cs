using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.Reporting.ProviderTypeReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
    }

    public class Csv
    {
        [Name("UKPRN")]
        public int ProviderUkprn { get; set; }

        [Name("Provider Name")]
        public string ProviderName { get; set; }

        [Name("Provider Type")]
        public int ProviderType { get; set; }

        [Name("Provider Type Description")]
        public string ProviderTypeDescription { get; set; }

        [Name("Provider Status")]
        public int ProviderStatus { get; set; }

        [Name("CD Provider Status")]
        public string ProviderStatusDescription { get; set; }

        [Name("Ukrlp Provider Status")]
        public string UkrlpProviderStatus { get; set; }

        [Name("Live Course Count")]
        public int LiveCourseCount { get; set; }

        [Name("Live Apprenticeship Count")]
        public int LiveApprenticeshipCount { get; set; }

        [Name("Live T Level Count")]
        public int LiveTLevelCount { get; set; }

    }

    public class Handler : IRequestHandler<Query, IAsyncEnumerable<Csv>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public Task<IAsyncEnumerable<Csv>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Process(_sqlQueryDispatcher.ExecuteQuery(new GetProviderTypeReport())));

            static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<ProviderTypeReportItem> results)
            {
                await foreach (var result in results)
                {
                    yield return new Csv
                    {
                        ProviderUkprn = result.ProviderUkprn,
                        ProviderName = result.ProviderName,
                        ProviderType = (int)result.ProviderType,
                        ProviderTypeDescription = string.Join("; ", result.ProviderType.SplitFlags().DefaultIfEmpty(ProviderType.None).Select(p => p.ToDescription())),
                        ProviderStatus = (int)result.ProviderStatus,
                        ProviderStatusDescription = result.ProviderStatus.ToString(),
                        UkrlpProviderStatus = result.UkrlpProviderStatusDescription,
                        LiveCourseCount = result.LiveCourseCount,
                        LiveApprenticeshipCount = result.LiveApprenticeshipCount,
                        LiveTLevelCount = result.LiveTLevelCount
                    };
                }
            }
        }
    }
}
