using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevelDefinitions
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel
    {
        public IReadOnlyCollection<TLevelDefinitionViewModel> TLevelDefinitions { get; set; }
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
            var tLevelDefinitions = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelDefinitions());

            return new ViewModel
            {
                TLevelDefinitions = tLevelDefinitions.Select(t => new TLevelDefinitionViewModel
                {
                    TLevelDefinitionId = t.TLevelDefinitionId,
                    FrameworkCode = t.FrameworkCode,
                    ProgType = t.ProgType,
                    QualificationLevel = t.QualificationLevel.ToString(),
                    Name = t.Name
                }).ToArray()
            };
        }
    }
}
