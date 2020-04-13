using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Validation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation
{
    public class FlowModel : IMptxState<IFlowModelCallback>
    {
        public Guid ProviderId { get; set; }
        public Guid? VenueId { get; set; }
        public int? Radius { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipDeliveryModes? DeliveryModes { get; set; }
    }

    public interface IFlowModelCallback : IMptxState
    {
        void ReceiveLocation(
            string instanceId,
            Guid venueId,
            bool national,
            int? radius,
            ApprenticeshipDeliveryModes deliveryModes);
    }

    public class Query : IRequest<ViewModel>
    {
        // For the callback from the Create Venue flow
        public Guid? VenueId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid? VenueId { get; set; }
        public int? Radius { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipDeliveryModes? DeliveryModes { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<(Guid venueId, string name)> Venues { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly MptxInstanceContext<FlowModel, IFlowModelCallback> _flow;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IProviderInfoCache providerInfoCache,
            MptxInstanceContext<FlowModel, IFlowModelCallback> flow)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _providerInfoCache = providerInfoCache;
            _flow = flow;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) =>
            CreateViewModel(await GetProviderVenues());

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var providerVenues = await GetProviderVenues();

            var validator = new Validator(providerVenues);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(providerVenues);
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _flow.Update(s =>
            {
                s.DeliveryModes = request.DeliveryModes;
                s.National = request.National;
                s.Radius = request.Radius;
                s.VenueId = request.VenueId;
            });

            _flow.UpdateParent(s => s.ReceiveLocation(
                _flow.InstanceId,
                _flow.State.VenueId.Value,
                _flow.State.National.Value,
                !_flow.State.National.Value ? _flow.State.Radius : null,
                _flow.State.DeliveryModes.Value));

            return new Success();
        }

        private ViewModel CreateViewModel(IReadOnlyCollection<Venue> providerVenues) => new ViewModel()
        {
            Venues = providerVenues.Select(v => (v.Id, v.VenueName)).OrderBy(v => v.VenueName).ToList(),
            VenueId = _flow.State.VenueId.HasValue && providerVenues.Any(v => v.Id == _flow.State.VenueId) ?
                _flow.State.VenueId :
                null,
            Radius = _flow.State.Radius,
            National = _flow.State.National,
            DeliveryModes = _flow.State.DeliveryModes
        };

        private async Task<IReadOnlyCollection<Venue>> GetProviderVenues()
        {
            var provider = await _providerInfoCache.GetProviderInfo(_flow.State.ProviderId);

            return await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetAllVenuesForProvider()
                {
                    ProviderUkprn = provider.Ukprn
                });
        }

        private class Validator : AbstractValidator<Command>
        {
            public Validator(IReadOnlyCollection<Venue> providerVenues)
            {
                RuleFor(c => c.VenueId)
                    .NotEmpty()
                    .Must(id => providerVenues.Any(v => v.Id == id))
                    .WithMessageForAllRules("Select the location");

                RuleFor(c => c.Radius)
                    .NotEmpty()
                    .When(c => c.National.GetValueOrDefault() == false)
                    .WithMessage("Enter how far you are willing to travel from the selected location");

                RuleFor(c => c.DeliveryModes)
                    .NotEmpty()
                    .NotEqual(ApprenticeshipDeliveryModes.None)
                    .WithMessageForAllRules("Select at least one option from Day Release and Block Release");
            }
        }
    }
}
