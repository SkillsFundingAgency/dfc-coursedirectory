using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

namespace Dfc.CourseDirectory.FindACourseApi.Features.NonLarsSubType
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public IReadOnlyCollection<NonLarsSubTypeViewModel> NonLarsSubTypes { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var nonLarsSubtypes = await _sqlQueryDispatcher.ExecuteQuery(new GetAllNonLarsSubTypes());

            return new ViewModel
            {
                NonLarsSubTypes = nonLarsSubtypes.Select(t => new NonLarsSubTypeViewModel
                {
                    NonLarsSubTypeId = t.NonLarsSubTypeId,
                    Name = t.Name
                }).ToArray()
            };
        }
    }
}
