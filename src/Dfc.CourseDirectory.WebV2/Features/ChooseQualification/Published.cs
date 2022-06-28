using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using MediatR;
using OneOf;
using OneOf.Types;


namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification.Published
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;

        public Handler(MptxInstanceContext<FlowModel> flow)
        {
            _flow = flow;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            //ThrowIfFlowStateNotValid();

            return Task.FromResult(new ViewModel()
            {
                CourseName = _flow.State.CourseName
            });
        }

        //        private void ThrowIfFlowStateNotValid()
        //        {
        //            _journeyInstance.ThrowIfNotCompleted();
        //        }
    }
}
