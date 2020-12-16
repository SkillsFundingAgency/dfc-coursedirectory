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

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel.CheckAndPublish
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public Guid TLevelId { get; set; }
        public string TLevelName { get; set; }
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
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly JourneyInstance<EditTLevelJourneyModel> _journeyInstance;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            JourneyInstance<EditTLevelJourneyModel> journeyInstance,
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

            var currentUser = _currentUserProvider.GetCurrentUser();
            var now = _clock.UtcNow;

            var result = await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateTLevel()
                {
                    UpdatedBy = currentUser,
                    UpdatedOn = now,
                    EntryRequirements = _journeyInstance.State.EntryRequirements,
                    HowYoullBeAssessed = _journeyInstance.State.HowYoullBeAssessed,
                    HowYoullLearn = _journeyInstance.State.HowYoullLearn,
                    LocationVenueIds = _journeyInstance.State.LocationVenueIds,
                    StartDate = _journeyInstance.State.StartDate.Value,
                    TLevelId = _journeyInstance.State.TLevelId,
                    Website = _journeyInstance.State.Website,
                    WhatYouCanDoNext = _journeyInstance.State.WhatYouCanDoNext,
                    WhatYoullLearn = _journeyInstance.State.WhatYoullLearn,
                    WhoFor = _journeyInstance.State.WhoFor,
                    YourReference = _journeyInstance.State.YourReference
                });

            if (result.Value is UpdateTLevelFailedReason reason)
            {
                if (reason == UpdateTLevelFailedReason.TLevelAlreadyExistsForDate)
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
                    $"Unknown {nameof(UpdateTLevelFailedReason)}: '{reason}'.");
            }

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
                TLevelId = _journeyInstance.State.TLevelId,
                TLevelName = _journeyInstance.State.TLevelName,
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
            if (!_journeyInstance.State.IsValid)
            {
                throw new InvalidStateException();
            }
        }
    }
}
