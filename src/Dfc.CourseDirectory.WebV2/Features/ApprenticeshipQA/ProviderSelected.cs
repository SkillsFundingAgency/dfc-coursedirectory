using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ProviderSelected
{
    using QueryResponse = OneOf<Error<ErrorReason>, ViewModel>;

    public enum ErrorReason
    {
        ProviderDoesNotExist,
        InvalidStatus
    }

    public class Query : IRequest<QueryResponse>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
        public bool ProviderAssessmentCompleted { get; set; }
        public IReadOnlyCollection<ViewModelApprenticeshipSubmission> ApprenticeshipAssessments { get; set; }
        public bool CanComplete { get; set; }
    }

    public class ViewModelApprenticeshipSubmission
    {
        public Guid ApprenticeshipId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public bool AssessmentCompleted { get; set; }
    }

    public class QueryHandler :
        IRequestHandler<Query, QueryResponse>,
        IRestrictQAStatus<Query>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public QueryHandler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Query>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress,
            ApprenticeshipQAStatus.Failed,
            ApprenticeshipQAStatus.Passed,
            ApprenticeshipQAStatus.UnableToComplete
        };

        public async Task<QueryResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            if (provider == null)
            {
                return new Error<ErrorReason>(ErrorReason.ProviderDoesNotExist);
            }

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = request.ProviderId
                });

            var maybeLatestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            if (maybeLatestSubmission.Value is None)
            {
                return new Error<ErrorReason>(ErrorReason.InvalidStatus);
            }

            var latestSubmission = maybeLatestSubmission.AsT1;

            var canComplete = qaStatus == ApprenticeshipQAStatus.InProgress && latestSubmission.Passed != null;

            return new ViewModel()
            {
                ApprenticeshipAssessments = latestSubmission.Apprenticeships
                    .Select(a => new ViewModelApprenticeshipSubmission()
                    {
                        ApprenticeshipId = a.ApprenticeshipId,
                        ApprenticeshipTitle = a.ApprenticeshipTitle,
                        AssessmentCompleted = latestSubmission.ApprenticeshipAssessmentsPassed != null
                    })
                    .ToList(),
                ApprenticeshipQAStatus = qaStatus,
                CanComplete = canComplete,
                ProviderAssessmentCompleted = latestSubmission.ProviderAssessmentPassed != null,
                ProviderId = request.ProviderId,
                ProviderName = provider.ProviderName
            };
        }

        Task<Guid> IRestrictQAStatus<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);
    }
}
