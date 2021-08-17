using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

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
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public CommandHandler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Command>.PermittedStatuses => new[]
        {
            ApprenticeshipQAStatus.InProgress
        };

        public async Task<ViewModel> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var provider = await _sqlQueryDispatcher.ExecuteQuery(
                new Core.DataStore.Sql.Queries.GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            if (provider == null)
            {
                throw new InvalidStateException(InvalidStateReason.ProviderDoesNotExist);
            }

            var latestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            if (latestSubmission == null)
            {
                // Belt & braces - should never happen
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQASubmission);
            }

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
                    await _sqlQueryDispatcher.ExecuteQuery(new PublishApprenticeship()
                    {
                        ApprenticeshipId = app.ApprenticeshipId,
                        PublishedBy = _currentUserProvider.GetCurrentUser(),
                        PublishedOn = _clock.UtcNow
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
