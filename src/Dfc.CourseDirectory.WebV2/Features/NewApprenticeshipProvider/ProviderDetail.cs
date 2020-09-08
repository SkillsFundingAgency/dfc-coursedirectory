using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.ProviderValidation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ProviderDetail
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
        public string ProviderName { get; set; }
    }

    public class ConfirmationQuery : IRequest<ConfirmationViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class ConfirmationViewModel : ConfirmationCommand
    {
        public string ProviderName { get; set; }
        public string CourseDirectoryStatus { get; set; }
        public int Ukprn { get; set; }
        public string TradingName { get; set; }
        public ProviderType ProviderType { get; set; }
        public string MarketingInformation { get; set; }
    }

    public class ConfirmationCommand : IRequest<Success>
    {
        public Guid ProviderId { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequireUserCanSubmitQASubmission<Query>,
        IRequestHandler<Command, CommandResponse>,
        IRequireUserCanSubmitQASubmission<Command>,
        IRequestHandler<ConfirmationQuery, ConfirmationViewModel>,
        IRequireUserCanSubmitQASubmission<ConfirmationQuery>,
        IRequestHandler<ConfirmationCommand, Success>,
        IRequireUserCanSubmitQASubmission<ConfirmationCommand>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            MptxInstanceContext<FlowModel> flow,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _flow = flow;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

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

            _flow.Update(s => s.SetProviderDetails(Html.SanitizeHtml(request.MarketingInformation)));

            return new Success();
        }

        public async Task<ConfirmationViewModel> Handle(ConfirmationQuery request, CancellationToken cancellationToken)
        {
            if (!_flow.State.GotProviderDetails)
            {
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            return new ConfirmationViewModel()
            {
                ProviderId = request.ProviderId,
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
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new UpdateProviderInfo()
                {
                    ProviderId = request.ProviderId,
                    MarketingInformation = _flow.State.ProviderMarketingInformation,
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
                MarketingInformation = _flow.State.ProviderMarketingInformation,
                ProviderName = provider.ProviderName
            };
        }

        Guid IRequireUserCanSubmitQASubmission<Query>.GetProviderId(Query request) => request.ProviderId;

        Guid IRequireUserCanSubmitQASubmission<Command>.GetProviderId(Command request) => request.ProviderId;

        Guid IRequireUserCanSubmitQASubmission<ConfirmationQuery>.GetProviderId(ConfirmationQuery request) => request.ProviderId;

        Guid IRequireUserCanSubmitQASubmission<ConfirmationCommand>.GetProviderId(ConfirmationCommand request) => request.ProviderId;

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.MarketingInformation).MarketingInformation();
            }
        }
    }
}
