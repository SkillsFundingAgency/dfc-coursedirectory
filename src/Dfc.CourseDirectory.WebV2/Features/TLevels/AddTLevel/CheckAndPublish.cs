using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation.Results;
using FormFlow;
using GovUk.Frontend.AspNetCore;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel.CheckAndPublish
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
        public string YourReference { get; set; }
        public Date StartDate { get; set; }
        public IReadOnlyCollection<string> LocationVenueNames { get; set; }
        public string Website { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ProviderId { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly JourneyInstance<AddTLevelJourneyModel> _journeyInstance;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            JourneyInstance<AddTLevelJourneyModel> journeyInstance,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            ICurrentUserProvider currentUserProvider)
        {
            _journeyInstance = journeyInstance;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _currentUserProvider = currentUserProvider;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            return CreateViewModel();
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var tLevelId = Guid.NewGuid();
            var currentUser = _currentUserProvider.GetCurrentUser();
            var now = _clock.UtcNow;

            var result = await _sqlQueryDispatcher.ExecuteQuery(
                new CreateTLevel()
                {
                    CreatedBy = currentUser,
                    CreatedOn = now,
                    EntryRequirements = _journeyInstance.State.EntryRequirements,
                    HowYoullBeAssessed = _journeyInstance.State.HowYoullBeAssessed,
                    HowYoullLearn = _journeyInstance.State.HowYoullLearn,
                    LocationVenueIds = _journeyInstance.State.LocationVenueIds,
                    ProviderId = request.ProviderId,
                    StartDate = _journeyInstance.State.StartDate.Value,
                    TLevelDefinitionId = _journeyInstance.State.TLevelDefinitionId.Value,
                    TLevelId = tLevelId,
                    Website = _journeyInstance.State.Website,
                    WhatYouCanDoNext = _journeyInstance.State.WhatYouCanDoNext,
                    WhatYoullLearn = _journeyInstance.State.WhatYoullLearn,
                    WhoFor = _journeyInstance.State.WhoFor,
                    YourReference = _journeyInstance.State.YourReference
                });

            if (result.Value is CreateTLevelFailedReason reason)
            {
                if (reason == CreateTLevelFailedReason.TLevelAlreadyExistsForDate)
                {
                    var vm = await CreateViewModel();

                    var validationResult = new ValidationResult(new[]
                    {
                        new ValidationFailure(
                            nameof(ViewModel.StartDate),
                            "T Level already exists, enter a new start date")
                    });

                    return new ModelWithErrors<ViewModel>(vm, validationResult);
                }

                throw new NotImplementedException(
                    $"Unknown {nameof(CreateTLevelFailedReason)}: '{reason}'.");
            }

            _journeyInstance.UpdateState(state => state.SetCreatedTLevel(tLevelId));

            // Complete JourneyInstance so state can no longer be changed
            _journeyInstance.Complete();

            return new Success();
        }

        private async Task<ViewModel> CreateViewModel()
        {
            var venues = await _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByIds()
                {
                    VenueIds = _journeyInstance.State.LocationVenueIds
                });

            return new ViewModel()
            {
                EntryRequirements = _journeyInstance.State.EntryRequirements,
                HowYoullBeAssessed = _journeyInstance.State.HowYoullBeAssessed,
                HowYoullLearn = _journeyInstance.State.HowYoullLearn,
                LocationVenueNames = _journeyInstance.State.LocationVenueIds
                    .Select(id => venues[id].VenueName)
                    .ToArray(),
                StartDate = (Date)_journeyInstance.State.StartDate.Value,
                Website = _journeyInstance.State.Website,
                WhatYouCanDoNext = _journeyInstance.State.WhatYouCanDoNext,
                WhatYoullLearn = _journeyInstance.State.WhatYoullLearn,
                WhoFor = _journeyInstance.State.WhoFor,
                YourReference = _journeyInstance.State.YourReference
            };
        }

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfCompleted();

            if (!_journeyInstance.State.CompletedStages.HasFlags(AddTLevelJourneyCompletedStages.All))
            {
                throw new InvalidStateException();
            }
        }
    }
}
