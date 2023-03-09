using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.HelpdeskDashboard
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
        
        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            return new ViewModel();
        }
    }
}
