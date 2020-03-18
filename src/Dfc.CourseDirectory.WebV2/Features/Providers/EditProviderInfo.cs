using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
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

namespace Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderInfo
{
    using CommandResponse = OneOf<ModelWithErrors<CommandViewModel>, Success>;
    using QueryResponse = OneOf<ModelWithErrors<CommandViewModel>, CommandViewModel>;

    public class Query : IRequest<QueryResponse>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
        public string Alias { get; set; }
        public string MarketingInformation { get; set; }
        public string CourseDirectoryName { get; set; }
    }

    public class CommandViewModel : Command
    {
        public bool ShowMarketingInformation { get; set; }
        public bool MarketingInformationIsEditable { get; set; }
        public bool CourseDirectoryNameIsEditable { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, QueryResponse>,
        IRequestHandler<Command, CommandResponse>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly CommandValidator _validator;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _validator = new CommandValidator(CourseDirectoryNameIsEditable, MarketingInformationIsEditable);
        }

        private bool CourseDirectoryNameIsEditable =>
            AuthorizationRules.CanUpdateProviderCourseDirectoryName(_currentUserProvider.GetCurrentUser());

        private bool MarketingInformationIsEditable =>
            AuthorizationRules.CanUpdateProviderMarketingInformation(_currentUserProvider.GetCurrentUser());

        public async Task<QueryResponse> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var vm = await CreateViewModel(request.ProviderId);

            var validationResult = await _validator.ValidateAsync(vm);

            return new ModelWithErrors<CommandViewModel>(vm, validationResult);
        }

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel(request.ProviderId);
                request.Adapt(vm);

                return new ModelWithErrors<CommandViewModel>(vm, validationResult);
            }

            var currentUser = _currentUserProvider.GetCurrentUser();

            var updateCommand = new UpdateProviderInfo()
            {
                ProviderId = request.ProviderId,
                Alias = request.Alias,
                MarketingInformation = MarketingInformationIsEditable ?
                    OneOf<None, string>.FromT1(request.MarketingInformation) :
                    new None(),
                CourseDirectoryName = CourseDirectoryNameIsEditable ?
                    OneOf<None, string>.FromT1(request.CourseDirectoryName) :
                    new None(),
                UpdatedBy = currentUser,
                UpdatedOn = _clock.UtcNow
            };
            await _cosmosDbQueryDispatcher.ExecuteQuery(updateCommand);

            return new Success();
        }

        private async Task<CommandViewModel> CreateViewModel(Guid providerId)
        {
            var provider = await GetProvider(providerId);

            var showBriefOverview = (provider.ProviderType & ProviderType.Apprenticeships) != 0;

            return new CommandViewModel()
            {
                Alias = provider.Alias,
                MarketingInformation = provider.MarketingInformation,
                MarketingInformationIsEditable = MarketingInformationIsEditable,
                ShowMarketingInformation = showBriefOverview,
                CourseDirectoryName = !string.IsNullOrEmpty(provider.CourseDirectoryName) ?
                    provider.CourseDirectoryName :
                    provider.ProviderName,
                CourseDirectoryNameIsEditable = CourseDirectoryNameIsEditable,
                ProviderId = providerId
            };
        }

        private async Task<Provider> GetProvider(Guid providerId)
        {
            var query = new GetProviderById() { ProviderId = providerId };
            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(query);

            if (result == null)
            {
                throw new ErrorException<ProviderDoesNotExist>(new ProviderDoesNotExist());
            }

            return result;
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(
                bool courseDirectoryNameIsEditable,
                bool marketingInformationIsEditable)
            {
                RuleFor(c => c.Alias).Alias();

                if (courseDirectoryNameIsEditable)
                {
                    RuleFor(c => c.CourseDirectoryName).CourseDirectoryName();
                }

                if (marketingInformationIsEditable)
                {
                    RuleFor(c => c.MarketingInformation).MarketingInformation();
                }
            }
        }
    }
}
