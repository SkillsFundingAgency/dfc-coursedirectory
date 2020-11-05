using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Complete
{
    public struct InvalidSubmission
    {
    }

    public class Command : IRequest<ViewModel>
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

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Command>.PermittedStatuses => new[]
        {
            ApprenticeshipQAStatus.InProgress
        };

        public async Task<ViewModel> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Core.DataStore.CosmosDb.Queries.GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            if (provider == null)
            {
                throw new InvalidStateException(InvalidStateReason.ProviderDoesNotExist);
            }

            var maybeLatestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            if (maybeLatestSubmission.Value is None)
            {
                // Belt & braces - should never happen
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQASubmission);
            }

            var latestSubmission = maybeLatestSubmission.AsT1;

            if (!latestSubmission.Passed.HasValue)
            {
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQASubmission);
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

            if (newStatus == ApprenticeshipQAStatus.Passed)
            {
                foreach (var app in latestSubmission.Apprenticeships)
                {
                    await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateApprenticeshipStatus()
                    {
                        ApprenticeshipId = app.ApprenticeshipId,
                        ProviderUkprn = provider.Ukprn,
                        Status = 1  // Live
                    });
                }
            }

            return new ViewModel()
            {
                ProviderName = provider.ProviderName
            };
        }

        Guid IRestrictQAStatus<Command>.GetProviderId(Command request) => request.ProviderId;
    }
}
