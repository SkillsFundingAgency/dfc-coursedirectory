using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.ViewComponents.EditTLevel;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.ViewModels.TLevels.EditTLevel.Save
{
    public class Command : IRequest
    {
        public string YourReference { get; set; }
        public DateOnly? StartDate { get; set; }
        public IList<Guid> LocationVenueIds { get; set; }
        public string Website { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly JourneyInstance<EditTLevelJourneyModel> _journeyInstance;

        public Handler(JourneyInstance<EditTLevelJourneyModel> journeyInstance)
        {
            _journeyInstance = journeyInstance;
        }

        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            // Stash the values in state
            // Since these haven't been validated `IsValid` is set to false
            // to ensure the un-validated data cannot be submitted

            _journeyInstance.UpdateState(state =>
            {
                state.YourReference = request.YourReference;
                state.StartDate = new DateTime(
                    request.StartDate.Value.Year
                    , request.StartDate.Value.Month
                    , request.StartDate.Value.Day
                );
                state.LocationVenueIds = request.LocationVenueIds;
                state.Website = request.Website;
                state.WhoFor = request.WhoFor;
                state.EntryRequirements = request.EntryRequirements;
                state.WhatYoullLearn = request.WhatYoullLearn;
                state.HowYoullBeAssessed = request.HowYoullBeAssessed;
                state.HowYoullLearn = request.HowYoullLearn;
                state.WhatYouCanDoNext = request.WhatYouCanDoNext;
                state.IsValid = false;
            });

            return Unit.Task;
        }
    }
}
