using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.Status
{
    using QueryResponse = OneOf<Error<ErrorReason>, Command>;
    using CommandResponse = OneOf<Error<ErrorReason>, ModelWithErrors<Command>, Success>;

    public enum ErrorReason
    {
        ProviderDoesNotExist,
        InvalidStatus
    }

    public class Query : IRequest<QueryResponse>
    {
        public Guid ProviderId { get; set; }
    }
    
    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
        public bool UnableToComplete { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons UnableToCompleteReasons { get; set; }
        public string Comments { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, QueryResponse>,
        IRequestHandler<Command, CommandResponse>
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

        public async Task<QueryResponse> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var errorOrData = await CheckStatus(request.ProviderId);

            if (errorOrData.Value is Error<ErrorReason>)
            {
                return errorOrData.AsT0;
            }

            var data = errorOrData.AsT1;

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
            var errorOrData = await CheckStatus(request.ProviderId);

            if (errorOrData.Value is Error<ErrorReason>)
            {
                return errorOrData.AsT0;
            }

            var data = errorOrData.AsT1;

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
                        Comments = request.Comments,
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

        private async Task<OneOf<Error<ErrorReason>, Data>> CheckStatus(Guid providerId)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            if (provider == null)
            {
                return new Error<ErrorReason>(ErrorReason.ProviderDoesNotExist);
            }

            var currentStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            if (currentStatus == ApprenticeshipQAStatus.Passed)
            {
                return new Error<ErrorReason>(ErrorReason.InvalidStatus);
            }

            return new Data()
            {
                ApprenticeshipQAStatus = currentStatus
            };
        }

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
                    .When(c => c.UnableToComplete)
                    .WithMessageForAllRules("Enter comments for the reason selected");
            }
        }
    }
}
