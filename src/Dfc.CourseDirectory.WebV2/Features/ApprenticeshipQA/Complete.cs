using System;
using System.Threading;
using System.Threading.Tasks;
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
    public enum ErrorReason
    {
        ProviderDoesNotExist,
        InvalidStatus
    }

    public class Command : IRequest<OneOf<Error<ErrorReason>, ViewModel>>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public string ProviderName { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, OneOf<Error<ErrorReason>, ViewModel>>
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

        public async Task<OneOf<Error<ErrorReason>, ViewModel>> Handle(
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

            var currentStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = request.ProviderId
                });

            if (currentStatus != ApprenticeshipQAStatus.InProgress)
            {
                return new Error<ErrorReason>(ErrorReason.InvalidStatus);
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
    }
}
