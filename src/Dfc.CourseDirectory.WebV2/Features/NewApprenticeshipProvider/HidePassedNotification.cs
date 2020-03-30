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
            var qasubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            var submission = qasubmission.Match(
                _ => throw new Exception(""),
                sub => sub);

            var hideDialog = await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateHidePassedNotification()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    HidePassedNotification = true
                });

            var resp = hideDialog.Match(
                notfound => throw new ErrorException<InvalidQASubmission>(new InvalidQASubmission()),
                success => success);

            return resp;
        }
    }

}