using System;
using System.Threading;
using System.Threading.Tasks;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel.Published
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public Guid TLevelId { get; set; }
        public string YourReference { get; set; }
        public string TLevelName { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly JourneyInstance<EditTLevelJourneyModel> _journeyInstance;

        public Handler(JourneyInstance<EditTLevelJourneyModel> journeyInstance)
        {
            _journeyInstance = journeyInstance;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) =>
            Task.FromResult(new ViewModel()
            {
                TLevelId = _journeyInstance.State.TLevelId,
                YourReference = _journeyInstance.State.YourReference,
                TLevelName=_journeyInstance.State.TLevelName
            });
    }
}
