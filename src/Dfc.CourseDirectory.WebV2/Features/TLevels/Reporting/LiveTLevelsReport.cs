using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.Reporting.LiveTLevelsReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
    }

    public class Csv
    {
        [Name("UKPRN")]
        public int Ukprn { get; set; }

        [Name("Provider Name")]
        public string ProviderName { get; set; }

        [Name("TLevel Name")]
        public string TLevelName { get; set; }

        [Name("Venue Name")]
        public string VenueName { get; set; }

        [Name("Start Date")]
        [Format("u")]
        public DateTime StartDate { get; set; }
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
            return Task.FromResult(Process(_sqlQueryDispatcher.ExecuteQuery(new GetLiveTLevelsReport())));

            static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<LiveTLevelsReportItem> results)
            {
                await foreach (var result in results)
                {
                    yield return new Csv
                    {
                        Ukprn = result.Ukprn,
                        ProviderName = result.ProviderName,
                        TLevelName = result.TLevelName,
                        VenueName = result.VenueName,
                        StartDate = result.StartDate
                    };
                }
            }
        }
    }
}
