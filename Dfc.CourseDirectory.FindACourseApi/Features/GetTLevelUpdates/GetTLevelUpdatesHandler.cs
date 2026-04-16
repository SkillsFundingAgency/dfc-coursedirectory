using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelUpdates
{
    public class TLevelUpdateResponse 
    {
        public int totalCount { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public DateTime cutOffDate { get; set; }
        public IList<TLevelUpdatesViewModel> courses { get; set; }
    }
    public class TLevelUpdateRequest : IRequest<OneOf<NotFound, TLevelUpdateResponse>>
    {
        public DateTime CutOffDate { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class Handler : IRequestHandler<TLevelUpdateRequest, OneOf<NotFound, TLevelUpdateResponse>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));
        }

        public async Task<OneOf<NotFound, TLevelUpdateResponse>> Handle(TLevelUpdateRequest request, CancellationToken cancellationToken)
        {
            var listOfTLevels = await _sqlQueryDispatcher.ExecuteQuery(new Core.DataStore.Sql.Queries.GetTLevelUpdates() { CutOffDate = request.CutOffDate, PageNumber = request.PageNumber, PageSize = request.PageSize });
            var response = new TLevelUpdateResponse()
            {
                totalCount = listOfTLevels.TLevelCount,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
                cutOffDate = request.CutOffDate,
                courses = listOfTLevels.TLevels.Select(c => new TLevelUpdatesViewModel()
                {
                    TLevelId = c.TLevelId,
                    UpdateType = c.UpdateType, 
                    CourseName = c.CourseName,
                    StartDate = c.StartDate,
                    CourseWebsite = c.CourseWebsite,
                    WhoTheCourseIsFor = c.WhoTheCourseIsFor,
                    EntryRequirements = c.EntryRequirements,
                    WhatYoullLearn = c.WhatYoullLearn,
                    HowYoullLearn = c.HowYoullLearn,
                    WhatYoullNeed = c.WhatYoullNeed,
                    HowYoullBeAssessed = c.HowYoullBeAssessed,
                    WhatYouCanDoNext = c.WhatYouCanDoNext,
                    ProviderName = c.ProviderName,
                    ProviderWebsite = c.ProviderWebsite,
                    ProviderEmail = c.ProviderEmail,
                    ProviderPhoneNumber= c.ProviderPhoneNumber,       
                    VenueName = c.VenueName,
                    Postcode = c.Postcode,
                    AddressLine1 = c.AddressLine1,
                    AddressLine2 = c.AddressLine2,
                    Town = c.Town,
                    County = c.County,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    QualificationLevel = c.QualificationLevel
                }).ToList()
            };
            return response;
        }
    }
}
