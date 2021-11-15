using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.OpenData.Reporting.LiveRegionsReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
        public DateTime FromDate { get; set; }
    }

    public class Csv
    {
        [Name("REGION_ID")] public string RegionId { get; set; }

        [Name("SUBREGION_NAME")] public string RegionName { get; set; }

        [Name("LATITUDE")] public double Latitude { get; set; }

        [Name("LONGITUDE")] public double Longitude { get; set; }

        [Name("POSTCODE")] public string Postcode { get; set; }
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

            return Task.FromResult(Process(_sqlQueryDispatcher.ExecuteQuery(new GetRegions())));

            async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<RegionItem> results)
            {
                await foreach (var result in results.WithCancellation(cancellationToken))
                {
                    yield return new Csv
                    {
                        RegionId = result.RegionId,
                        RegionName = result.RegionName,
                        Latitude = result.Latitude,
                        Longitude = result.Longitude,
                        Postcode = result.Postcode,
                    };
                }
            }
        }
    }
}
