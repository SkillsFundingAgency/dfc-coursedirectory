﻿using System.Threading;
using System.Threading.Tasks;
using FormFlow;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Published
{
    public class Query : IRequest<ViewModel>
    {
        public bool IsNonLars { get; set; }
    }

    public class ViewModel
    {
        public bool IsNonLars { get; set; }
        public int PublishedCount { get; set; }
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
            var journeyInstance = _journeyInstanceProvider.GetInstance<PublishJourneyModel>();

            return Task.FromResult(new ViewModel()
            {
                IsNonLars = request.IsNonLars,
                PublishedCount = journeyInstance.State.CoursesPublished
            });
        }
    }
}
