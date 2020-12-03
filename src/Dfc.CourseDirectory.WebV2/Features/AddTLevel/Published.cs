using System;
using System.Threading;
using System.Threading.Tasks;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.AddTLevel.Published
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public Guid TLevelId { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly JourneyInstance<AddTLevelJourneyModel> _journeyInstance;

        public Handler(JourneyInstance<AddTLevelJourneyModel> journeyInstance)
        {
            _journeyInstance = journeyInstance;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            return Task.FromResult(new ViewModel()
            {
                TLevelId = _journeyInstance.State.TLevelId.Value
            });
        }

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfNotCompleted();
        }
    }
}
