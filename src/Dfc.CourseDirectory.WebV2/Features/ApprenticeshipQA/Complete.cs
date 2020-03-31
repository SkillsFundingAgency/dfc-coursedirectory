using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Complete
{
    public struct InvalidSubmission
    {
    }

    public class Command : IRequest<ViewModel>, IProviderScopedRequest
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public string ProviderName { get; set; }
    }

    public class CommandHandler :
        IRequestHandler<Command, ViewModel>,
        IRestrictQAStatus<Command>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public CommandHandler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public IEnumerable<ApprenticeshipQAStatus> PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.InProgress
        };

        public async Task<ViewModel> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            if (provider == null)
            {
                throw new ErrorException<ProviderDoesNotExist>(new ProviderDoesNotExist());
            }

            var maybeLatestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            if (maybeLatestSubmission.Value is None)
            {
                // Belt & braces - should never happen
                throw new ErrorException<InvalidSubmission>(new InvalidSubmission());
            }

            var latestSubmission = maybeLatestSubmission.AsT1;

            if (!latestSubmission.Passed.HasValue)
            {
                throw new ErrorException<InvalidSubmission>(new InvalidSubmission());
            }

            var newStatus = latestSubmission.Passed.Value ?
                ApprenticeshipQAStatus.Passed :
                ApprenticeshipQAStatus.Failed;

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = request.ProviderId,
                    ApprenticeshipQAStatus = newStatus
                });

            return new ViewModel()
            {
                ProviderName = provider.ProviderName
            };
        }
    }
}
