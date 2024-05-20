using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.TLevelValidation;
using FluentValidation;
using FormFlow;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel.Details
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ProviderId { get; set; }
        public string YourReference { get; set; }
        public DateInput StartDate { get; set; }
        public HashSet<Guid> LocationVenueIds { get; set; }
        public string Website { get; set; }
        // If any additional data is added here be sure to replicate in SaveDetails.Command
    }

    public class ViewModel : Command
    {
        public string TLevelName { get; set; }
        public IReadOnlyCollection<ViewModelProviderVenuesItem> ProviderVenues { get; set; }
    }

    public class ViewModelProviderVenuesItem
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly JourneyInstance<AddTLevelJourneyModel> _journeyInstance;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            JourneyInstance<AddTLevelJourneyModel> journeyInstance,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _journeyInstance = journeyInstance;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var providerVenues = await GetVenuesForProvider(request.ProviderId);
            return CreateViewModel(request.ProviderId, providerVenues);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var providerVenues = await GetVenuesForProvider(request.ProviderId);

            // Remove any invalid venue IDs
            request.LocationVenueIds ??= new HashSet<Guid>();
            request.LocationVenueIds.Intersect(providerVenues.Select(v => v.VenueId));

            var validator = new CommandValidator(
                request.ProviderId,
                _journeyInstance.State.TLevelDefinitionId.Value,
                _sqlQueryDispatcher);

            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(request.ProviderId, providerVenues);
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _journeyInstance.UpdateState(state => state.SetDetails(
                request.YourReference,
                request.StartDate.Value,
                request.LocationVenueIds,
                request.Website,
                isComplete: true));

            return new Success();
        }

        private ViewModel CreateViewModel(Guid providerId, IReadOnlyCollection<Venue> providerVenues) =>
            new ViewModel()
            {
                ProviderId = providerId,
                ProviderVenues = providerVenues
                    .Select(v => new ViewModelProviderVenuesItem()
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName
                    })
                    .ToList(),
                LocationVenueIds = new HashSet<Guid>(_journeyInstance.State.LocationVenueIds),
                StartDate = _journeyInstance.State.StartDate,
                TLevelName = _journeyInstance.State.TLevelName,
                Website = _journeyInstance.State.Website,
                YourReference = _journeyInstance.State.YourReference
            };

        private  Task<IReadOnlyCollection<Venue>> GetVenuesForProvider(Guid providerId) =>
            _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByProvider()
                {
                    ProviderId = providerId
                });

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfCompleted();

            if (!_journeyInstance.State.CompletedStages.HasFlags(
                AddTLevelJourneyCompletedStages.SelectTLevel,
                AddTLevelJourneyCompletedStages.Description))
            {
                throw new InvalidStateException();
            }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(Guid providerId, Guid tLevelDefinitionId, ISqlQueryDispatcher sqlQueryDispatcher)
            {
                RuleFor(c => c.YourReference).YourReference();

                RuleFor(c => c.StartDate)
                    .StartDate(tLevelId: null, providerId, tLevelDefinitionId, sqlQueryDispatcher);

                RuleFor(c => c.LocationVenueIds)
                    .NotEmpty()
                        .WithMessage("Select a T Level venue");

                RuleFor(c => c.Website).Website();
            }
        }
    }
}
