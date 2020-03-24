﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Validation;
using Dfc.CourseDirectory.WebV2.Validation.ProviderValidation;
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

    public class ConfirmationQuery : IRequest<ConfirmationViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class ConfirmationViewModel : ConfirmationCommand
    {
        public string ProviderName { get; set; }
        public string CourseDirectoryName { get; set; }
        public int Ukprn { get; set; }
        public string LegalName { get; set; }
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

            _flow.Update(s => s.SetProviderDetail(Html.SanitizeHtml(request.MarketingInformation)));

            return new Success();
        }

        public async Task<ConfirmationViewModel> Handle(ConfirmationQuery request, CancellationToken cancellationToken)
        {
            if (!_flow.State.GotProviderDetail)
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
                CourseDirectoryName = GetCourseDirectoryName(provider),
                LegalName = provider.ProviderName,
                MarketingInformation = _flow.State.ProviderMarketingInformation,
                ProviderType = provider.ProviderType,
                TradingName = provider.TradingName,
                Ukprn = provider.Ukprn
            };
        }

        public async Task<Success> Handle(ConfirmationCommand request, CancellationToken cancellationToken)
        {
            if (!_flow.State.GotProviderDetail)
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

        private static string GetCourseDirectoryName(Provider provider) =>
            !string.IsNullOrEmpty(provider.CourseDirectoryName) ?
                provider.CourseDirectoryName :
                provider.ProviderName;

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
                CourseDirectoryName = GetCourseDirectoryName(provider)
            };
        }

        Task<Guid> IRequireUserCanSubmitQASubmission<Query>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRequireUserCanSubmitQASubmission<Command>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRequireUserCanSubmitQASubmission<ConfirmationQuery>.GetProviderId(ConfirmationQuery request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRequireUserCanSubmitQASubmission<ConfirmationCommand>.GetProviderId(ConfirmationCommand request) =>
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
