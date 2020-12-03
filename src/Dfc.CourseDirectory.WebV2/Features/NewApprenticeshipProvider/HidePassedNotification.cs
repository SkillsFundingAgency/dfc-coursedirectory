using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.HidePassedNotification
{
    using CommandResponse = OneOf<ModelWithErrors<CommandViewModel>, Success>;

    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
    }

    public class CommandViewModel : Command
    {
    }

    public class CommandHandler : IRequestHandler<Command, CommandResponse>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CommandHandler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var currentUser = _currentUserProvider.GetCurrentUser();
            if (currentUser.IsHelpdesk)
            {
                throw new NotAuthorizedException();
            }

            var latestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            if (latestSubmission == null)
            {
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQASubmission);
            }

            // Cannot hide passed notification if qa status is not passed
            if (latestSubmission.Passed != true)
            {
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQAStatus);
            }

            // Cannot hide notification if it has already been hidden
            if (latestSubmission.HidePassedNotification)
            {
                throw new InvalidStateException();
            }

            // Hide notification
            await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateHidePassedNotification()
                {
                    ApprenticeshipQASubmissionId = latestSubmission.ApprenticeshipQASubmissionId,
                    HidePassedNotification = true
                });

            return new Success();
        }
    }
}
