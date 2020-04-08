using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Report
{
    public class Query : IRequest<ViewModel> { }


    public class ViewModel
    {
        public IReadOnlyCollection<ReportModel> Report { get; set; }
    }

    public class ReportModel
    {
        public Guid ProviderId { get; set; }
        public string UKPRN { get; set; }
        public string ProviderName { get; set; }
        public string Email { get; set; }
        [CsvHelper.Configuration.Attributes.Name("DPassed QA")]
        public string PassedQA => QAStatus == ApprenticeshipQAStatus.Passed ? "Yes" : "No";
        [CsvHelper.Configuration.Attributes.Name("Date Passed")]
        public string PassedQAOn { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Failed QA")]
        public string FailedQA => QAStatus == ApprenticeshipQAStatus.Failed ? "Yes" : "No";
        [CsvHelper.Configuration.Attributes.Name("Date Failed")]
        public string FailedQAOn { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Unable to complete")]
        public string UnableToComplete => QAStatus == ApprenticeshipQAStatus.UnableToComplete ? "Yes" : "No";
        [CsvHelper.Configuration.Attributes.Name("Date Unable to complete")]
        public string UnableToCompleteOn { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Why can't they complete")]
        public string UnableToCompleteReasons { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Tribal Notes")]
        public string Notes { get; set; }
        public ApprenticeshipQAStatus? QAStatus { get; set; }
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
                         select new ReportModel()
                         {
                             ProviderId = r.ProviderId,
                             Email = r.Email,
                             PassedQAOn = r.PassedQAOn.HasValue ? r.PassedQAOn.Value.ToString("dd MMM yyyy") : null,
                             FailedQAOn = r.FailedQAOn.HasValue ? r.FailedQAOn.Value.ToString("dd MMM yyyy") : null,
                             UnableToCompleteReasons = r.UnableToCompleteReasons.HasValue ? string.Join(",", EnumHelper.SplitFlags(r.UnableToCompleteReasons.Value).ToList().Select(x => x.ToDisplayName()).ToList()) : null,
                             UnableToCompleteOn = r.UnabletoCompleteOn.HasValue ? r.UnabletoCompleteOn.Value.ToString("dd MMM yyyy") : null,
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
