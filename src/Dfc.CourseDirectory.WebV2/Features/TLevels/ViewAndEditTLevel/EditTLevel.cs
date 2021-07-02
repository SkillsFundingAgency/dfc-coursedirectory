using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel.EditTLevel
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public string YourReference { get; set; }
        public DateInput StartDate { get; set; }
        public HashSet<Guid> LocationVenueIds { get; set; }
        public string Website { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
    }

    public class ViewModel : Command
    {
        public Guid TLevelId { get; set; }
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
        private readonly JourneyInstance<EditTLevelJourneyModel> _journeyInstance;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            JourneyInstance<EditTLevelJourneyModel> journeyInstance,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _journeyInstance = journeyInstance;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerVenues = await GetVenuesForProvider();
            return CreateViewModel(providerVenues);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var providerVenues = await GetVenuesForProvider();

            // Remove any invalid venue IDs
            request.LocationVenueIds ??= new HashSet<Guid>();
            request.LocationVenueIds.Intersect(providerVenues.Select(v => v.VenueId));

            var validator = new CommandValidator(
                _journeyInstance.State.ProviderId,
                _journeyInstance.State.TLevelDefinitionId,
                _sqlQueryDispatcher);

            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(providerVenues);
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _journeyInstance.UpdateState(state =>
            {
                state.YourReference = request.YourReference;
                state.StartDate = request.StartDate.Value;
                state.LocationVenueIds = request.LocationVenueIds.ToList();
                state.Website = request.Website;
                state.WhoFor = request.WhoFor;
                state.EntryRequirements = request.EntryRequirements;
                state.WhatYoullLearn = request.WhatYoullLearn;
                state.HowYoullBeAssessed = request.HowYoullBeAssessed;
                state.HowYoullLearn = request.HowYoullLearn;
                state.WhatYouCanDoNext = request.WhatYouCanDoNext;
                state.IsValid = true;
            });

            return new Success();
        }

        private ViewModel CreateViewModel(IReadOnlyCollection<Venue> providerVenues) => new ViewModel()
        {
            ProviderVenues = providerVenues
                .Select(v => new ViewModelProviderVenuesItem()
                {
                    VenueId = v.VenueId,
                    VenueName = v.VenueName
                })
                .ToList(),
            LocationVenueIds = new HashSet<Guid>(_journeyInstance.State.LocationVenueIds),
            StartDate = _journeyInstance.State.StartDate,
            Website = _journeyInstance.State.Website,
            YourReference = _journeyInstance.State.YourReference,
            EntryRequirements = _journeyInstance.State.EntryRequirements,
            HowYoullBeAssessed = _journeyInstance.State.HowYoullBeAssessed,
            HowYoullLearn = _journeyInstance.State.HowYoullLearn,
            TLevelId = _journeyInstance.State.TLevelId,
            TLevelName = _journeyInstance.State.TLevelName,
            WhatYouCanDoNext = _journeyInstance.State.WhatYouCanDoNext,
            WhatYoullLearn = _journeyInstance.State.WhatYoullLearn,
            WhoFor = _journeyInstance.State.WhoFor
        };

        private Task<IReadOnlyCollection<Venue>> GetVenuesForProvider() =>
            _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByProvider()
                {
                    ProviderId = _journeyInstance.State.ProviderId
                });

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(Guid providerId, Guid tLevelDefinitionId, ISqlQueryDispatcher sqlQueryDispatcher)
            {
                RuleFor(c => c.YourReference).YourReference();

                RuleFor(c => c.StartDate)
                    .StartDate(providerId, tLevelDefinitionId, sqlQueryDispatcher);

                RuleFor(c => c.LocationVenueIds)
                    .NotEmpty()
                        .WithMessage("Select a T Level venue");

                RuleFor(c => c.Website).Website();

                RuleFor(c => c.WhoFor).WhoFor();
                RuleFor(c => c.EntryRequirements).EntryRequirements();
                RuleFor(c => c.WhatYoullLearn).WhatYoullLearn();
                RuleFor(c => c.HowYoullLearn).HowYoullLearn();
                RuleFor(c => c.HowYoullBeAssessed).HowYoullBeAssessed();
                RuleFor(c => c.WhatYouCanDoNext).WhatYouCanDoNext();
            }
        }
    }
}
