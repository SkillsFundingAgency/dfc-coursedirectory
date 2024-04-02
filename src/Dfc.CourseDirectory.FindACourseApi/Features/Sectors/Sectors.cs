using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.FindACourseApi.Features.Sectors
{
    public class Query : IRequest<IReadOnlyCollection<SectorViewModel>>
    {       
    }

    public class Handler : IRequestHandler<Query, IReadOnlyCollection<SectorViewModel>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));            
        }

        public async Task<IReadOnlyCollection<SectorViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var sectors = await _sqlQueryDispatcher.ExecuteQuery(new GetSectorsAttachedWithCourses());

            return sectors?.Select(s => new SectorViewModel { Id = s.Id, Code = s.Code, Description = s.Description }).ToList() ?? new List<SectorViewModel>();
        }
    }
}
