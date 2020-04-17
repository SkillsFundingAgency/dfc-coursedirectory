using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.Core.Validation;
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
        public ApprenticeshipDeliveryModes? DeliveryModes { get; set; }

        public static FlowModel Add(Guid providerId) => new FlowModel()
        {
            Mode = Mode.Add,
            ProviderId = providerId
        };

        public static FlowModel Edit(
            Guid providerId,
            Guid venueId,
            int radius,
            ApprenticeshipDeliveryModes deliveryModes) =>
            new FlowModel()
            {
                Mode = Mode.Edit,
                DeliveryModes = deliveryModes,
                ProviderId = providerId,
                Radius = radius,
                VenueId = venueId,
                OriginalVenueId = venueId
            };
    }

    public interface IFlowModelCallback : IMptxState
    {
        IReadOnlyCollection<Guid> BlockedVenueIds { get; }
        void RemoveLocation(Guid venueId);
        void ReceiveLocation(
            string instanceId,
            Guid venueId,
            Guid? originalVenueId,
            int radius,
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
        public ApprenticeshipDeliveryModes DeliveryModes { get; set; }
    }

    public class ViewModel : Command
    {
        public Mode Mode { get; set; }
        public IReadOnlyCollection<(Guid venueId, string name, bool blocked)> Venues { get; set; }
    }

    public class RemoveQuery : IRequest<RemoveViewModel>
    {
    }

    public class RemoveCommand : IRequest<Success>
    {
    }

    public class RemoveViewModel : RemoveCommand
    {
        public string VenueName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>,
        IRequestHandler<RemoveQuery, RemoveViewModel>,
        IRequestHandler<RemoveCommand, Success>
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
                s.Radius = request.Radius;
                s.VenueId = request.VenueId;
            });

            _flow.UpdateParent(s => s.ReceiveLocation(
                _flow.InstanceId,
                _flow.State.VenueId.Value,
                _flow.State.OriginalVenueId,
                _flow.State.Radius.Value,
                _flow.State.DeliveryModes.Value));

            return new Success();
        }

        public async Task<RemoveViewModel> Handle(RemoveQuery request, CancellationToken cancellationToken)
        {
            if (_flow.State.Mode != Mode.Edit)
            {
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }

            var providerVenues = await GetProviderVenues();
            var thisVenue = providerVenues.Single(v => v.Id == _flow.State.VenueId);

            return new RemoveViewModel()
            {
                VenueName = thisVenue.VenueName
            };
        }

        public Task<Success> Handle(RemoveCommand request, CancellationToken cancellationToken)
        {
            if (_flow.State.Mode != Mode.Edit)
            {
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }

            _flow.UpdateParent(s => s.RemoveLocation(_flow.State.VenueId.Value));

            _flow.Complete();

            return Task.FromResult(new Success());
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
                    .WithMessage("Enter how far you are willing to travel from the selected location");

                RuleFor(c => c.DeliveryModes)
                    .NotEmpty()
                    .NotEqual(ApprenticeshipDeliveryModes.None)
                    .WithMessageForAllRules("Select at least one option from Day Release and Block Release");
            }
        }
    }
}
