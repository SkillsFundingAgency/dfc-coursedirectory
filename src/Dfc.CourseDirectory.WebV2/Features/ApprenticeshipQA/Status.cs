using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Status
{
    using CommandResponse = OneOf<ModelWithErrors<Command>, Success>;

    public class Query : IRequest<Command>
    {
        public Guid ProviderId { get; set; }
    }
    
    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
        public bool UnableToComplete { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons UnableToCompleteReasons { get; set; }
        public string Comments { get; set; }
        public string StandardName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRestrictQAStatus<Query>,
        IRequestHandler<Command, CommandResponse>,
        IRestrictQAStatus<Command>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Command>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.NotStarted,
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress,
            ApprenticeshipQAStatus.Failed,
            ApprenticeshipQAStatus.UnableToComplete
        };

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Query>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.NotStarted,
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress,
            ApprenticeshipQAStatus.Failed,
            ApprenticeshipQAStatus.UnableToComplete
        };

        public async Task<Command> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var data = await CheckStatus(request.ProviderId);

            var unableToComplete = data.ApprenticeshipQAStatus.HasFlag(ApprenticeshipQAStatus.UnableToComplete);

            var info = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQAUnableToCompleteInfoForProvider()
                {
                    ProviderId = request.ProviderId
                });

            return new Command()
            {
                ProviderId = request.ProviderId,
                UnableToComplete = unableToComplete,
                Comments = unableToComplete ? info.AsT1.Comments : null,
                UnableToCompleteReasons = unableToComplete ? info.AsT1.UnableToCompleteReasons : ApprenticeshipQAUnableToCompleteReasons.None,
            };
        }

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var data = await CheckStatus(request.ProviderId);

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            var oldStatus = data.ApprenticeshipQAStatus;
            var newStatus = request.UnableToComplete ?
                oldStatus | ApprenticeshipQAStatus.UnableToComplete :
                oldStatus & ~ApprenticeshipQAStatus.UnableToComplete;

            if (request.UnableToComplete)
            {
                await _sqlQueryDispatcher.ExecuteQuery(
                    new CreateApprenticeshipQAUnableToCompleteInfo()
                    {
                        ProviderId = request.ProviderId,
                        UnableToCompleteReasons = request.UnableToCompleteReasons,
                        Comments = request.UnableToCompleteReasons.HasFlag(ApprenticeshipQAUnableToCompleteReasons.Other) ?
                            request.Comments :
                            null,
                        StandardName = request.UnableToCompleteReasons.HasFlag(ApprenticeshipQAUnableToCompleteReasons.StandardNotReady) ?
                            request.StandardName :
                            null,
                        AddedByUserId = _currentUserProvider.GetCurrentUser().UserId,
                        AddedOn = _clock.UtcNow
                    });
            }

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = request.ProviderId,
                    ApprenticeshipQAStatus = newStatus
                });

            return new Success();
        }

        private async Task<Data> CheckStatus(Guid providerId)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            if (provider == null)
            {
                throw new ErrorException<ProviderDoesNotExist>(new ProviderDoesNotExist());
            }

            var currentStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            return new Data()
            {
                ApprenticeshipQAStatus = currentStatus.ValueOrDefault()
            };
        }

        Guid IRestrictQAStatus<Query>.GetProviderId(Query request) => request.ProviderId;

        Guid IRestrictQAStatus<Command>.GetProviderId(Command request) => request.ProviderId;

        private class Data
        {
            public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.UnableToCompleteReasons)
                    .NotNull()
                    .NotEqual(ApprenticeshipQAUnableToCompleteReasons.None)
                    .When(c => c.UnableToComplete)
                    .WithMessageForAllRules("A reason must be selected");

                RuleFor(c => c.Comments)
                    .NotEmpty()
                    .When(c => c.UnableToComplete && c.UnableToCompleteReasons.HasFlag(ApprenticeshipQAUnableToCompleteReasons.Other))
                    .WithMessageForAllRules("Enter comments for the reason selected");

                RuleFor(c => c.StandardName)
                    .NotEmpty()
                    .When(c => c.UnableToComplete && c.UnableToCompleteReasons.HasFlag(ApprenticeshipQAUnableToCompleteReasons.StandardNotReady))
                    .WithMessageForAllRules("Enter the standard name");
            }
        }
    }
}
