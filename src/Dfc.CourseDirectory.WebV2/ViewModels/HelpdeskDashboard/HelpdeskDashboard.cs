using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.ViewModels.HelpdeskDashboard
{
    public class Query : IRequest<ViewModel>
    {
    }
    public class ViewModel
    {
        public int CourseNumber { get; set; }
        public int OutofDateCourses { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ViewModel());
        }
    }
}
