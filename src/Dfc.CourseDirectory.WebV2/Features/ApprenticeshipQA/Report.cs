using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Report
{
    public class Query : IRequest<ViewModel> { }


    public class ViewModel
    {
        public IReadOnlyCollection<QAStatusReport> Report { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public QueryHandler(ISqlQueryDispatcher sqlQueryDispatcher, ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetQAStatusReport());
            var infos = (from r in results
                         select new QAStatusReport()
                         {
                             ProviderId = r.ProviderId,
                             Email = r.Email,
                             PassedQAOn = r.PassedQAOn,
                             FailedQAOn =r.FailedQAOn,
                             UnableToCompleteReasons = r.UnableToCompleteReasons,
                             UnableToCompleteOn = r.UnabletoCompleteOn,
                             QAStatus = r.QAStatus,
                             Notes = r.Notes
                         }).ToList();

            var providers = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProvidersByIds()
            {
                ProviderIds = results.Select(r => r.ProviderId)
            });

            //map cosmos record
            infos.ForEach(x =>
            {
                x.UKPRN = providers[x.ProviderId].UnitedKingdomProviderReferenceNumber;
                x.ProviderName = providers[x.ProviderId].ProviderName;
            });

            return new ViewModel()
            {
                Report = infos
            };
        }
    }
}
