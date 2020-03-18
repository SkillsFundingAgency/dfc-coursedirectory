using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Validation;
using Dfc.CourseDirectory.WebV2.Validation.ProviderValidation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.MarketingInfo
{
    using CommandResponse = OneOf<ModelWithErrors<ViewModel>, Success>;

    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
        public string MarketingInformation { get; set; }
    }

    public class ViewModel : Command
    {
        public string CourseDirectoryName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRestrictProviderType<Query>,
        IRequireUserCanSubmitQASubmission<Query>,
        IRestrictQAStatus<Query>,
        IRequestHandler<Command, CommandResponse>,
        IRestrictProviderType<Command>,
        IRequireUserCanSubmitQASubmission<Command>,
        IRestrictQAStatus<Command>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Query>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.NotStarted
        };

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Command>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.NotStarted
        };

        ProviderType IRestrictProviderType<Query>.ProviderType => ProviderType.Apprenticeships;

        ProviderType IRestrictProviderType<Command>.ProviderType => ProviderType.Apprenticeships;

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) =>
            CreateViewModel(request.ProviderId);

        public async Task<CommandResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel(request.ProviderId);
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new UpdateProviderMarketingInfo()
                {
                    ProviderId = request.ProviderId,
                    MarketingInformation = request.MarketingInformation,
                    UpdatedBy = _currentUserProvider.GetCurrentUser(),
                    UpdatedOn = _clock.UtcNow
                });

            return new Success();
        }

        private async Task<ViewModel> CreateViewModel(Guid providerId)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            return new ViewModel()
            {
                ProviderId = provider.Id,
                MarketingInformation = Html.SanitizeHtml(provider.MarketingInformation ?? string.Empty),
                CourseDirectoryName = !string.IsNullOrEmpty(provider.CourseDirectoryName) ?
                    provider.CourseDirectoryName :
                    provider.ProviderName
            };
        }

        Task<Guid> IRequireUserCanSubmitQASubmission<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRequireUserCanSubmitQASubmission<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictQAStatus<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictQAStatus<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictProviderType<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictProviderType<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.MarketingInformation).MarketingInformation();
            }
        }
    }
}
