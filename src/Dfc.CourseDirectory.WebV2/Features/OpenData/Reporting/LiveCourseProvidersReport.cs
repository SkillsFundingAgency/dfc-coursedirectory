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

namespace Dfc.CourseDirectory.WebV2.Features.OpenData.Reporting.LiveCourseProvidersReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
        public DateTime FromDate { get; set; }
    }

    public class Csv
    {
        [Name("PROVIDER_UKPRN")]
        public string ProviderUkprn { get; set; }

        [Name("PROVIDER_NAME")]
        public string ProviderName { get; set; }

        [Name("TRADING_NAME")]
        public string TradingName { get; set; }

        [Name("CONTACT_ADDRESS1")]
        public string ContactAddress1 { get; set; }

        [Name("CONTACT_ADDRESS2")]
        public string ContactAddress2 { get; set; }

        [Name("CONTACT_TOWN")]
        public string ContactTown { get; set; }

        [Name("CONTACT_POSTCODE")]
        public string ContactPostcode { get; set; }

        [Name("CONTACT_WEBSITE")]
        public string ContactWebsite { get; set; }

        [Name("CONTACT_PHONE")]
        public string ContactPhone { get; set; }

        [Name("CONTACT_EMAIL")]
        public string ContactEmail { get; set; }
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
                
                return Task.FromResult(Process(_sqlQueryDispatcher.ExecuteQuery(new GetLiveCourseProvidersReport
                {
                    FromDate = request.FromDate
                })));

                static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<LiveCourseProvidersReportItem> results)
                {
                    await foreach (var result in results)
                    {
                        yield return new Csv
                        {
                            ProviderUkprn = result.Ukprn.ToString(),
                            ProviderName = result.ProviderName,
                            TradingName = result.TradingName,
                            ContactAddress1 = result.ContactAddress1,
                            ContactAddress2 = result.ContactAddress2,
                            ContactTown = result.AddressPostTown,
                            ContactPostcode = result.AddressPostcode,
                            ContactWebsite = result.WebsiteAddress,
                            ContactPhone = result.Telephone.ToString(),
                            ContactEmail = result.Email
                        };
                    }
                }
            }
        }
}
