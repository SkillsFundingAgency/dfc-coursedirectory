using System;
using System.Collections.Generic;
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

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Complete
{
    using CommandResponse = OneOf<Error<ErrorReason>, ViewModel>;

    public enum ErrorReason
    {
        ProviderDoesNotExist,
        InvalidStatus
    }

    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public string ProviderName { get; set; }
    }

    public class CommandHandler :
        IRequestHandler<Command, CommandResponse>,
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

        public async Task<CommandResponse> Handle(
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
                return new Error<ErrorReason>(ErrorReason.ProviderDoesNotExist);
            }

            var maybeLatestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            if (maybeLatestSubmission.Value is None)
            {
                // Belt & braces - should never happen
                return new Error<ErrorReason>(ErrorReason.InvalidStatus);
            }

            var latestSubmission = maybeLatestSubmission.AsT1;

            if (!latestSubmission.Passed.HasValue)
            {
                return new Error<ErrorReason>(ErrorReason.InvalidStatus);
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

        Task<Guid> IRestrictQAStatus<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);
    }
}
