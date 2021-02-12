using System.Threading;
using System.Threading.Tasks;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue.Published
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public string Name { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly JourneyInstance<AddVenueJourneyModel> _journeyInstance;

        public Handler(JourneyInstance<AddVenueJourneyModel> journeyInstance)
        {
            _journeyInstance = journeyInstance;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            _journeyInstance.ThrowIfNotCompleted();

            return Task.FromResult(new ViewModel()
            {
                Name = _journeyInstance.State.Name
            });
        }
    }
}
