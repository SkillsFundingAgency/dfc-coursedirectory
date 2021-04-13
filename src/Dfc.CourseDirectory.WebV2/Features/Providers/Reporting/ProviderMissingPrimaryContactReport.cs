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

namespace Dfc.CourseDirectory.WebV2.Features.Providers.Reporting.ProviderMissingPrimaryContactReport
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
            return Task.FromResult(Process(_sqlQueryDispatcher.ExecuteQuery(new GetProviderMissingPrimaryContactReport())));

            static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<ProviderMissingPrimaryContactReportItem> results)
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
                    };
                }
            }
        }
    }
}
