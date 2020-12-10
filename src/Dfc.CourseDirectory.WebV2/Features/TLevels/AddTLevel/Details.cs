using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.TLevelValidation;
using Dfc.CourseDirectory.WebV2.Validation;
using FluentValidation;
using FluentValidation.Results;
using FormFlow;
using GovUk.Frontend.AspNetCore;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        public Date? StartDate { get; set; }
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
        private readonly CommandValidator _validator;

        public Handler(
            JourneyInstance<AddTLevelJourneyModel> journeyInstance,
            ISqlQueryDispatcher sqlQueryDispatcher,
            CommandValidator validator)
        {
            _journeyInstance = journeyInstance;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _validator = validator;
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

            var validationResult = await _validator.ValidateAsync(request);

            // TODO Move this logic into Validation.TLevelValidation when Validator doesn't have to be injected.
            // This is a limitation of the way the current Date validation works that will go away when
            // tag helper library supports model binding to DateTimes.
            await CheckNoExistingTLevelForStartDate(validationResult, request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(request.ProviderId, providerVenues);
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _journeyInstance.UpdateState(state => state.SetDetails(
                request.YourReference,
                request.StartDate.Value.ToDateTime(),
                request.LocationVenueIds,
                request.Website,
                isComplete: true));

            return new Success();
        }

        private async Task CheckNoExistingTLevelForStartDate(ValidationResult validationResult, Command command)
        {
            if (!command.StartDate.HasValue)
            {
                return;
            }

            var existingTLevels = await _sqlQueryDispatcher.ExecuteQuery(
                new GetTLevelsForProvider() { ProviderId = command.ProviderId });

            if (existingTLevels.Any(tl =>
                tl.TLevelDefinition.TLevelDefinitionId == _journeyInstance.State.TLevelDefinitionId &&
                tl.StartDate == command.StartDate?.ToDateTime()))
            {
                validationResult.Errors.Add(
                    new ValidationFailure(nameof(Command.StartDate), "Start date already exists"));
            }
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
                StartDate = (Date?)_journeyInstance.State.StartDate,
                TLevelName = _journeyInstance.State.TLevelName,
                Website = _journeyInstance.State.Website,
                YourReference = _journeyInstance.State.YourReference
            };

        private  Task<IReadOnlyCollection<Venue>> GetVenuesForProvider(Guid providerId) =>
            _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesForProvider()
                {
                    ProviderId = providerId
                });

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfCompleted();

            if (!_journeyInstance.State.ValidStages.HasFlags(
                AddTLevelJourneyCompletedStages.SelectTLevel,
                AddTLevelJourneyCompletedStages.Description))
            {
                throw new InvalidStateException();
            }
        }
    }

    public class CommandValidator : ValidatorBase<Command>
    {
        public CommandValidator(IActionContextAccessor actionContextAccessor)
            : base(actionContextAccessor)
        {
            RuleFor(c => c.YourReference).YourReference();

            // TODO Move this logic into Validation.TLevelValidation when tag helper library
            // supports binding DateTimes
            RuleFor(c => c.StartDate)
                .Date(displayName: "Start date", missingErrorMessage: "Enter a start date", isRequired: true);

            RuleFor(c => c.LocationVenueIds)
                .NotEmpty()
                    .WithMessage("Select a T Level location");

            RuleFor(c => c.Website).Website();
        }
    }
}
