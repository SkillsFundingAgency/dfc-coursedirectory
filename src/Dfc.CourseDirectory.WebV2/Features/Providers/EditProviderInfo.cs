using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
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

namespace Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderInfo
{
    public class Query : IRequest<ModelWithErrors<CommandViewModel>>
    {
        public int Ukprn { get; set; }
    }

    public class Command : IRequest<OneOf<Success, ModelWithErrors<CommandViewModel>>>
    {
        public int Ukprn { get; set; }
        public string Alias { get; set; }
        public string BriefOverview { get; set; }
        public string CourseDirectoryName { get; set; }
    }

    public class CommandViewModel : Command
    {
        public bool ShowBriefOverview { get; set; }
        public bool BriefOverviewIsEditable { get; set; }
        public bool ShowCourseDirectoryName { get; set; }
        public bool CourseDirectoryNameIsEditable { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ModelWithErrors<CommandViewModel>>,
        IRequestHandler<Command, OneOf<Success, ModelWithErrors<CommandViewModel>>>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly CommandValidator _validator;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _validator = new CommandValidator();
        }

        public async Task<ModelWithErrors<CommandViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await GetProvider(request.Ukprn);
            var qaStatus = await GetProviderApprenticeshipQAStatus(provider.Id);

            var command = new Command()
            {
                Ukprn = int.Parse(provider.UnitedKingdomProviderReferenceNumber),
                BriefOverview = provider.MarketingInformation,
                Alias = provider.Alias,
                CourseDirectoryName = !string.IsNullOrEmpty(provider.CourseDirectoryName) ? provider.CourseDirectoryName : provider.ProviderName
            };

            var vm = CreateViewModel(command, qaStatus, provider.ProviderType);
            var validationResult = await _validator.ValidateAsync(vm);
            return new ModelWithErrors<CommandViewModel>(vm, validationResult);
        }

        public async Task<OneOf<Success, ModelWithErrors<CommandViewModel>>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var provider = await GetProvider(request.Ukprn);
            var qaStatus = await GetProviderApprenticeshipQAStatus(provider.Id);
            var currentUser = _currentUserProvider.GetCurrentUser();

            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var viewModel = CreateViewModel(request, qaStatus, provider.ProviderType);
                return new ModelWithErrors<CommandViewModel>(viewModel, validationResult);
            }

            var updateCommand = new UpdateProviderInfo()
            {
                ProviderId = provider.Id,
                Alias = request.Alias,
                BriefOverview = ProviderValidation.BriefOverviewIsEditable(qaStatus, currentUser) ?
                    OneOf<string, None>.FromT0(request.BriefOverview) :
                    new None(),
                CourseDirectoryName = ProviderValidation.CourseDirectoryNameIsEditable(currentUser) ?
                    OneOf<string, None>.FromT0(request.CourseDirectoryName) :
                    new None(),
                UpdatedBy = currentUser,
                UpdatedOn = _clock.UtcNow
            };
            await _cosmosDbQueryDispatcher.ExecuteQuery(updateCommand);

            return new Success();
        }

        private CommandViewModel CreateViewModel(
            Command command,
            ApprenticeshipQAStatus qaStatus,
            ProviderType providerType)
        {
            var currentUser = _currentUserProvider.GetCurrentUser();

            var showBriefOverview = (providerType & ProviderType.Apprenticeships) != 0;

            return new CommandViewModel()
            {
                Alias = command.Alias,
                BriefOverview = command.BriefOverview,
                BriefOverviewIsEditable = ProviderValidation.BriefOverviewIsEditable(qaStatus, currentUser),
                CourseDirectoryName = command.CourseDirectoryName,
                CourseDirectoryNameIsEditable = ProviderValidation.CourseDirectoryNameIsEditable(currentUser),
                ShowCourseDirectoryName = ProviderValidation.CourseDirectoryNameIsEditable(currentUser),
                ShowBriefOverview = showBriefOverview,
                Ukprn = command.Ukprn
            };
        }

        private async Task<Provider> GetProvider(int ukprn)
        {
            var query = new GetProviderByUkprn() { Ukprn = ukprn };
            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(query);

            if (result == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider);
            }

            return result;
        }

        private Task<ApprenticeshipQAStatus> GetProviderApprenticeshipQAStatus(Guid providerId)
        {
            var query = new GetProviderApprenticeshipQAStatus() { ProviderId = providerId };
            return _sqlQueryDispatcher.ExecuteQuery(query);
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Alias).RuleForAlias();
                RuleFor(c => c.CourseDirectoryName).RuleForCourseDirectoryName();
                // TODO BriefOverview
            }
        }
    }
}
