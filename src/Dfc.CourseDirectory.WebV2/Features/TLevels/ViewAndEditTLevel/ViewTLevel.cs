using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel.ViewTLevel
{
    public class Query : IRequest<OneOf<NotFound, ViewModel>>
    {
        public Guid TLevelId { get; set; }
    }

    public class ViewModel
    {
        public Guid TLevelId { get; set; }
        public string TLevelDefinitionName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string YourReference { get; set; }
        public DateTime StartDate { get; set; }
        public IReadOnlyCollection<string> LocationVenueNames { get; set; }
        public string Website { get; set; }
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<NotFound, ViewModel>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<NotFound, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevel { TLevelId = request.TLevelId });

            if (result == null)
            {
                return new NotFound();
            }

            return new ViewModel
            { 
                TLevelId = result.TLevelId,
                TLevelDefinitionName = result.TLevelDefinition.Name,
                CreatedOn = result.CreatedOn,
                UpdatedOn = result.UpdatedOn,
                YourReference = result.YourReference,
                StartDate = result.StartDate,
                LocationVenueNames = result.Locations.Select(l => l.VenueName).ToArray(),
                Website = result.Website,
                WhoFor = result.WhoFor,
                EntryRequirements = result.EntryRequirements,
                WhatYoullLearn = result.WhatYoullLearn,
                HowYoullLearn = result.HowYoullLearn,
                HowYoullBeAssessed = result.HowYoullBeAssessed,
                WhatYouCanDoNext = result.WhatYouCanDoNext
            };
        }
    }
}
