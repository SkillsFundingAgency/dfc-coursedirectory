using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Validation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.HidePassedNotification
{
    using CommandResponse = OneOf<ModelWithErrors<CommandViewModel>, Success>;

    public class Command : IRequest<CommandResponse>, IProviderScopedRequest
    {
        public Guid ProviderId { get; set; }
    }

    public class CommandViewModel : Command
    {
    }

    public class CommandHandler :
        IRequestHandler<Command, CommandResponse>,
        IRestrictProviderType<Command>
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

        public ProviderType ProviderType => ProviderType.Apprenticeships;

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var currentUser = _currentUserProvider.GetCurrentUser();
            if (currentUser.IsHelpdesk)
            {
                throw new NotAuthorizedException();
            }

            var maybeSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            var submission = maybeSubmission.Match(
                notfound => throw new ErrorException<InvalidQASubmission>(new InvalidQASubmission()),
                found => found);

            // Cannot hide passed notification if qa status is not passed
            if (submission.Passed != true)
            {
                throw new ErrorException<InvalidQAStatus>(new InvalidQAStatus());
            }

            // Cannot hide notification if it has already been hidden
            if (submission.HidePassedNotification)
            {
                throw new ErrorException<PassedNotificationAlreadyHidden>(new PassedNotificationAlreadyHidden());
            }

            // Hide notification
            var hideDialog = await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateHidePassedNotification()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    HidePassedNotification = true
                });

            return hideDialog.Match(
                notfound => throw new ErrorException<InvalidQASubmission>(new InvalidQASubmission()),
                success => success);
        }
    }

}