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

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelList
{
    public class TLevelResponse 
    {
        public int totalTLevelCount { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public IList<TLevelListViewModel> tLevels { get; set; }
    }
    public class TLevelRequest : IRequest<OneOf<NotFound, TLevelResponse>>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class Handler : IRequestHandler<TLevelRequest, OneOf<NotFound, TLevelResponse>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));
        }

        public async Task<OneOf<NotFound, TLevelResponse>> Handle(TLevelRequest request, CancellationToken cancellationToken)
        {
            var listOfTLevels = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelsList() { PageNumber = request.PageNumber, PageSize = request.PageSize });
            var response = new TLevelResponse()
            {
                totalTLevelCount = listOfTLevels.TLevelsCount,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
                tLevels = listOfTLevels.TLevels.Select(c => new TLevelListViewModel()
                {
                    TLevelId = c.TLevelId,
                    CourseName = c.CourseName,
                    TlevelQualificationLevel =  c.TlevelQualificationLevel ,
                    WhoTheCourseIsFor = c.WhoTheCourseIsFor,
                    EntryRequirements = c.EntryRequirements,
                    WhatYoullLearn = c.WhatYoullLearn,
                    HowYoullLearn = c.HowYoullLearn,
                    HowYoullBeAssessed = c.HowYoullBeAssessed,
                    WhatYouCanDoNext = c.WhatYouCanDoNext,
                    CourseWebsite = c.CourseWebsite,
                    ProviderName = c.ProviderName,
                    ProviderWebsite = c.ProviderWebsite,
                    ProviderEmail = c.ProviderEmail,
                    ProviderPhoneNumber = c.ProviderPhoneNumber,
                    VenueName = c.VenueName,
                    Postcode = c.Postcode,
                    AddressLine1 = c.AddressLine1,
                    AddressLine2 = c.AddressLine2,
                    Town = c.Town,
                    County = c.County,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude

                }).ToList()
            };
            return response;
        }
    }
}
