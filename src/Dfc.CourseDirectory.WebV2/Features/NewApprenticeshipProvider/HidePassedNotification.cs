using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Validation;
using MediatR;
using OneOf;
using OneOf.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public CommandHandler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            //either return a submission of throw if provider does not have a submission
            var qasubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            var submission = qasubmission.Match(
                notfound => throw new ErrorException<InvalidQASubmission>(new InvalidQASubmission()),
                found => found);

            //cannot hide passed notification if qa status is not passed
            if (submission.Passed != true)
            {
                throw new ErrorException<InvalidQAStatus>(new InvalidQAStatus());
            }

            //cannot hide notification if it has already been hidden
            if (submission.HidePassedNotification)
            {
                throw new ErrorException<PassedNotificationAlreadyHidden>(new PassedNotificationAlreadyHidden());
            }

            //hide notification
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