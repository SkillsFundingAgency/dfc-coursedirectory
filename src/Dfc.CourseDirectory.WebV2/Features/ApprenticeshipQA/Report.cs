﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Report
{
    public class Query : IRequest<ViewModel> { }


    public class ViewModel
    {
        public IReadOnlyCollection<ReportModel> Report { get; set; }
    }

    public class ReportModel
    {
        [CsvHelper.Configuration.Attributes.Ignore]
        public Guid ProviderId { get; set; }
        [CsvHelper.Configuration.Attributes.Name("UKPRN")]
        public int Ukprn { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Name of provider")]
        public string ProviderName { get; set; }
        public string Email { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Passed QA")]
        public string PassedQA { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Date Passed")]
        public string PassedQAOn { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Failed QA")]
        public string FailedQA { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Date Failed")]
        public string FailedQAOn { get; set; }
        [CsvHelper.Configuration.Attributes.Name("QA Status")]
        public string QAStatus { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Unable to complete")]
        public string UnableToComplete { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Date Unable to complete")]
        public string UnableToCompleteOn { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Tribal Notes")]
        public string Notes { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Why can't they complete")]
        public string UnableToCompleteReasons { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public QueryHandler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetQAStatusReport());
            var infos = (from r in results
                         select new ReportModel()
                         {
                             ProviderId = r.ProviderId,
                             ProviderName = r.ProviderName,
                             Ukprn = r.Ukprn,
                             Email = r.Email,
                             PassedQAOn = r.PassedQAOn.HasValue ? r.PassedQAOn.Value.ToString("dd MMM yyyy") : null,
                             FailedQAOn = r.FailedQAOn.HasValue ? r.FailedQAOn.Value.ToString("dd MMM yyyy") : null,
                             UnableToCompleteReasons = r.UnableToCompleteReasons.HasValue ? string.Join(",", EnumHelper.SplitFlags(r.UnableToCompleteReasons.Value).ToList().Select(x => x.ToDescription()).ToList()) : null,
                             UnableToCompleteOn = r.UnabletoCompleteOn.HasValue ? r.UnabletoCompleteOn.Value.ToString("dd MMM yyyy") : null,
                             Notes = r.Notes,
                             QAStatus = r.QAStatus.HasValue ? r.QAStatus.Value.ToDescription() : null,
                             PassedQA = r.QAStatus == ApprenticeshipQAStatus.Passed ? "Yes" : "No",
                             FailedQA = r.QAStatus == ApprenticeshipQAStatus.Failed ? "Yes" : "No",
                             UnableToComplete = r.QAStatus.HasValue ? (r.QAStatus.Value.HasFlag(ApprenticeshipQAStatus.UnableToComplete) ? "Yes" : "No") : "No"
                         }).ToList();

            return new ViewModel()
            {
                Report = infos
            };
        }
    }
}
