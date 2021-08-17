using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.ProviderValidation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ProviderDetail
{
    using CommandResponse = OneOf<ModelWithErrors<ViewModel>, Success>;

    public class Query : IRequest<ViewModel>
    {
    }

    public class Command : IRequest<CommandResponse>
    {
        public string MarketingInformation { get; set; }
    }

    public class ViewModel : Command
    {
        public string ProviderName { get; set; }
    }

    public class ConfirmationQuery : IRequest<ConfirmationViewModel>
    {
    }

    public class ConfirmationViewModel : ConfirmationCommand
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string CourseDirectoryStatus { get; set; }
        public int Ukprn { get; set; }
        public string TradingName { get; set; }
        public ProviderType ProviderType { get; set; }
        public string MarketingInformation { get; set; }
    }

    public class ConfirmationCommand : IRequest<Success>
    {
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, CommandResponse>,
        IRequestHandler<ConfirmationQuery, ConfirmationViewModel>,
        IRequestHandler<ConfirmationCommand, Success>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            MptxInstanceContext<FlowModel> flow,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _flow = flow;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) =>
            CreateViewModel();

        public async Task<CommandResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel();
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _flow.Update(s => s.SetProviderDetails(Html.SanitizeHtml(request.MarketingInformation)));

            return new Success();
        }

        public async Task<ConfirmationViewModel> Handle(ConfirmationQuery request, CancellationToken cancellationToken)
        {
            if (!_flow.State.GotProviderDetails)
            {
                throw new InvalidStateException();
            }

            var providerId = _providerContextProvider.GetProviderId();

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            return new ConfirmationViewModel()
            {
                ProviderId = providerId,
                ProviderName = provider.ProviderName,
                CourseDirectoryStatus = provider.ProviderStatus,
                MarketingInformation = _flow.State.ProviderMarketingInformation,
                ProviderType = provider.ProviderType,
                TradingName = provider.Alias,
                Ukprn = provider.Ukprn
            };
        }

        public async Task<Success> Handle(ConfirmationCommand request, CancellationToken cancellationToken)
        {
            if (!_flow.State.GotProviderDetails)
            {
                throw new InvalidStateException();
            }

            var providerId = _providerContextProvider.GetProviderId();

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new UpdateProviderInfo()
                {
                    ProviderId = providerId,
                    MarketingInformation = _flow.State.ProviderMarketingInformation,
                    UpdatedBy = _currentUserProvider.GetCurrentUser(),
                    UpdatedOn = _clock.UtcNow
                });

            return new Success();
        }

        private async Task<ViewModel> CreateViewModel()
        {
            var providerId = _providerContextProvider.GetProviderId();

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            return new ViewModel()
            {
                MarketingInformation = _flow.State.ProviderMarketingInformation,
                ProviderName = provider.ProviderName
            };
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.MarketingInformation).MarketingInformation();
            }
        }
    }
}
