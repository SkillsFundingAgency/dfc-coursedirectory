using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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
        public int ProviderUkprn { get; set; }

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
                return Task.FromResult(Process(_sqlQueryDispatcher.ExecuteQuery(new GetLiveCourseProvidersReport())));

                static async IAsyncEnumerable<Csv> Process(IAsyncEnumerable<LiveCourseProvidersReportItem> results)
                {
                    await foreach (var result in results)
                    {
                        yield return new Csv
                        {
                            ProviderUkprn = result.ProviderUkprn,
                            ProviderName = result.ProviderName,
                            TradingName = result.TradingName,
                            ContactAddress1 = ParseAddress(result.AddressSaonDescription, result.AddressPaonDescription, result.AddressStreetDescription),
                            ContactAddress2 = result.AddressLocality,
                            ContactTown = !string.IsNullOrWhiteSpace(result.AddressPostTown)
                                ? result.AddressItems
                                : result.AddressPostTown,
                            ContactPostcode = result.AddressPostcode,
                            ContactWebsite = result.WebsiteAddress,
                            ContactPhone = !string.IsNullOrWhiteSpace(result.Telephone1)
                                ? result.Telephone2
                                : result.Telephone1,
                            ContactEmail = result.Email
                        };
                    }
                }
            }

            private static string ParseAddress(string saon, string paon, string street)
            {
                var addressParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(saon))
                    addressParts.Add(saon.Trim());

                if (!string.IsNullOrWhiteSpace(paon))
                    addressParts.Add(paon.Trim());

                if (!string.IsNullOrWhiteSpace(street))
                    addressParts.Add(street.Trim());

                return string.Join(", ", addressParts);
            }
        }
}
