﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ListProviders
{
    public class Query : IRequest<ViewModel> { }

    public class ViewModel
    {
        public IReadOnlyCollection<ProviderApprenticeshipQAInfo> Submitted { get; set; }
        public IReadOnlyCollection<ProviderApprenticeshipQAInfo> InProgress { get; set; }
        public IReadOnlyCollection<ProviderApprenticeshipQAInfo> Failed { get; set; }
        public IReadOnlyCollection<ProviderApprenticeshipQAInfo> UnableToComplete { get; set; }
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
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderApprenticeshipQAInfoByStatus()
            {
                Statuses = new[]
                {
                    ApprenticeshipQAStatus.Submitted,
                    ApprenticeshipQAStatus.Failed,
                    ApprenticeshipQAStatus.InProgress,
                    ApprenticeshipQAStatus.UnableToComplete
                }
            });

            var providers = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProvidersByIds()
            {
                ProviderIds = results.Select(r => r.ProviderId)
            });

            var infos = (from r in results
                         let provider = providers[r.ProviderId]
                         select new ProviderApprenticeshipQAInfo()
                         {
                             ApprenticeshipQAStatus = r.ApprenticeshipQAStatus,
                             LastAssessedBy = r.LastAssessedBy,
                             ProviderId = r.ProviderId,
                             ProviderName = provider.ProviderName,
                             ProviderUkprn = int.Parse(provider.UnitedKingdomProviderReferenceNumber),
                             SubmittedOn = r.SubmittedOn
                         }).ToList();

            var submitted = infos.Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Submitted);
            var failed = infos.Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Failed);
            var inProgress = infos.Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.InProgress);
            var unableToComplete = infos.Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.UnableToComplete);

            return new ViewModel()
            {
                Submitted = submitted.ToList(),
                Failed = failed.ToList(),
                InProgress = inProgress.ToList(),
                UnableToComplete = unableToComplete.ToList()
            };
        }
    }
}
