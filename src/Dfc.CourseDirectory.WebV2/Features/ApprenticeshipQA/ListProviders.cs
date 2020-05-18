using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ListProviders
{
    public class Query : IRequest<ViewModel> { }

    public class ViewModel
    {
        public IReadOnlyCollection<ViewModelResult> NotStarted { get; set; }
        public IReadOnlyCollection<ViewModelResult> Submitted { get; set; }
        public IReadOnlyCollection<ViewModelResult> InProgress { get; set; }
        public IReadOnlyCollection<ViewModelResult> Failed { get; set; }
        public IReadOnlyCollection<ViewModelResult> UnableToComplete { get; set; }
    }

    public class ViewModelResult
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int ProviderUkprn { get; set; }
        public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public UserInfo LastAssessedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons? UnableToCompleteReasons { get; set; }
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
                    ApprenticeshipQAStatus.NotStarted,
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
                         where provider.ProviderType.HasFlag(ProviderType.Apprenticeships)
                         select new ViewModelResult()
                         {
                             ApprenticeshipQAStatus = r.ApprenticeshipQAStatus,
                             LastAssessedBy = r.LastAssessedBy,
                             ProviderId = r.ProviderId,
                             ProviderName = provider.ProviderName,
                             ProviderUkprn = int.Parse(provider.UnitedKingdomProviderReferenceNumber),
                             SubmittedOn = r.SubmittedOn,
                             AddedOn = r.AddedOn,
                             UnableToCompleteReasons = r.UnableToCompleteReasons
                         }).ToList();

            var unableToComplete = infos
                .Where(i => i.ApprenticeshipQAStatus.HasFlag(ApprenticeshipQAStatus.UnableToComplete))
                .OrderByDescending(i => i.AddedOn)
                .ToList();

            var notStarted = infos
                .Where(i => !unableToComplete.Contains(i))
                .Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.NotStarted)
                .OrderByDescending(i => i.AddedOn)
                .ToList();

            var submitted = infos
                .Where(i => !unableToComplete.Contains(i))
                .Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Submitted)
                .OrderByDescending(i => i.SubmittedOn)
                .ToList();

            var failed = infos
                .Where(i => !unableToComplete.Contains(i))
                .Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.Failed)
                .OrderByDescending(i => i.SubmittedOn)
                .ToList();

            var inProgress = infos
                .Where(i => !unableToComplete.Contains(i))
                .Where(i => i.ApprenticeshipQAStatus == ApprenticeshipQAStatus.InProgress)
                .OrderByDescending(i => i.SubmittedOn)
                .ToList();

            return new ViewModel()
            {
                NotStarted = notStarted,
                Submitted = submitted,
                Failed = failed,
                InProgress = inProgress,
                UnableToComplete = unableToComplete
            };
        }
    }
}
