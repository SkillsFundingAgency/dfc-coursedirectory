using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.ViewComponents.Venues.AddVenue;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.ViewModels.Venues.AddVenue.Published
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
        private readonly JourneyInstanceProvider _journeyInstanceProvider;

        public Handler(JourneyInstanceProvider journeyInstanceProvider)
        {
            _journeyInstanceProvider = journeyInstanceProvider;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            journeyInstance.ThrowIfNotCompleted();

            return Task.FromResult(new ViewModel()
            {
                Name = journeyInstance.State.Name
            });
        }
    }
}
