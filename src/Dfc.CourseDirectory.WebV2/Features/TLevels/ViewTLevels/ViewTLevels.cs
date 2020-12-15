using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewTLevels
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public IReadOnlyCollection<TLevelViewModel> TLevels { get; set; }
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
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelsForProvider { ProviderId = request.ProviderId });

            return new ViewModel
            {
                TLevels = results.Select(t => new TLevelViewModel
                {
                    TLevelId = t.TLevelId,
                    Name = t.TLevelDefinition.Name,
                    StartDate = t.StartDate,
                    VenueNames = t.Locations.Select(l => l.VenueName).ToArray()
                }).ToArray()
            };
        }
    }
}
