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
    public enum Mode
    {
        Add,
        Edit
    }

    public class FlowModel : IMptxState<IFlowModelCallback>
    {
        private FlowModel() { }

        public Mode Mode { get; set; }
        public Guid ProviderId { get; set; }
        public Guid? VenueId { get; set; }
        public Guid? OriginalVenueId { get; set; }
        public int? Radius { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipDeliveryModes? DeliveryModes { get; set; }

        public static FlowModel Add(Guid providerId) => new FlowModel()
        {
            Mode = Mode.Add,
            ProviderId = providerId
        };

        public static FlowModel Edit(
            Guid providerId,
            Guid venueId,
            bool national,
            int? radius,
            ApprenticeshipDeliveryModes deliveryModes) =>
            new FlowModel()
            {
                Mode = Mode.Edit,
                DeliveryModes = deliveryModes,
                National = national,
                ProviderId = providerId,
                Radius = radius,
                VenueId = venueId,
                OriginalVenueId = venueId
            };
    }

    public interface IFlowModelCallback : IMptxState
    {
        IReadOnlyCollection<Guid> BlockedVenueIds { get; }
        void ReceiveLocation(
            string instanceId,
            Guid venueId,
            Guid? originalVenueId,
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
        public ApprenticeshipDeliveryModes DeliveryModes { get; set; }
    }

    public class ViewModel : Command
    {
        public Mode Mode { get; set; }
        public IReadOnlyCollection<(Guid venueId, string name, bool blocked)> Venues { get; set; }
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

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerVenues = await GetProviderVenues();
            var blockedVenueIds = GetNormalizedBlockedVenueIds();

            var vm = CreateViewModel(providerVenues, blockedVenueIds);
            
            if (request.VenueId.HasValue)
            {
                vm.VenueId = request.VenueId;
            }

            return vm;
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var providerVenues = await GetProviderVenues();
            var blockedVenueIds = GetNormalizedBlockedVenueIds();

            var validator = new Validator(providerVenues, blockedVenueIds);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(providerVenues, blockedVenueIds);
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
                _flow.State.OriginalVenueId,
                _flow.State.National.GetValueOrDefault(),
                !_flow.State.National.GetValueOrDefault() ? _flow.State.Radius : null,
                _flow.State.DeliveryModes.Value));

            return new Success();
        }

        private ViewModel CreateViewModel(IReadOnlyCollection<Venue> providerVenues, ISet<Guid> blockedVenueIds) =>
            new ViewModel()
            {
                Mode = _flow.State.Mode,
                Venues = providerVenues
                    .Select(v => (v.Id, v.VenueName, blocked: blockedVenueIds.Contains(v.Id)))
                    .OrderBy(v => v.VenueName)
                    .ToList(),
                VenueId = _flow.State.VenueId.HasValue && providerVenues.Any(v => v.Id == _flow.State.VenueId) ?
                    _flow.State.VenueId :
                    null,
                    Radius = _flow.State.Radius,
                    National = _flow.State.National,
                    DeliveryModes = _flow.State.DeliveryModes.GetValueOrDefault()
            };

        private ISet<Guid> GetNormalizedBlockedVenueIds()
        {
            var set = new HashSet<Guid>(_flow.ParentState.BlockedVenueIds ?? Array.Empty<Guid>());

            if (_flow.State.VenueId.HasValue)
            {
                set.Remove(_flow.State.VenueId.Value);
            }

            return set;
        }

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
            public Validator(IReadOnlyCollection<Venue> providerVenues, ISet<Guid> blockedVenueIds)
            {
                RuleFor(c => c.VenueId)
                    .Must(id => providerVenues.Select(v => v.Id).Except(blockedVenueIds).Contains(id.GetValueOrDefault()))
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
