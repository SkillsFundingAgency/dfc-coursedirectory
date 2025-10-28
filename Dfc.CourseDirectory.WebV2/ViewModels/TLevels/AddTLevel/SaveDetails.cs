﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.ViewModels.TLevels.AddTLevel.SaveDetails
{
    public class Command : IRequest
    {
        public string YourReference { get; set; }
        public DateOnly? StartDate { get; set; }
        public IList<Guid> LocationVenueIds { get; set; }
        public string Website { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly JourneyInstance<AddTLevelJourneyModel> _journeyInstance;

        public Handler(JourneyInstance<AddTLevelJourneyModel> journeyInstance)
        {
            _journeyInstance = journeyInstance;
        }

        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            // Stash the values in state
            // Since these haven't been validated `isComplete` is set to false
            // to ensure the un-validated data cannot be submitted
            _journeyInstance.UpdateState(state => state.SetDetails(
                request.YourReference,
                request.StartDate != null ? new DateTime(
                    request.StartDate.Value.Year
                    , request.StartDate.Value.Month
                    , request.StartDate.Value.Day
                ): (DateTime?) null,
                request.LocationVenueIds,
                request.Website,
                isComplete: false));

            return Unit.Task;
        }

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
    }
}
