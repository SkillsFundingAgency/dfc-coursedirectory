using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification.CourseSelected
{
    public class Command : IRequest
    {
        public string LarsCode { get; set; }
        public string CourseName { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;

        public Handler(MptxInstanceContext<FlowModel> flow)
        {
            _flow = flow;
        }

        public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            _flow.Update(s => s.SetCourse(request.LarsCode, request.CourseName));

            return Unit.Task;
        }
    }
}
