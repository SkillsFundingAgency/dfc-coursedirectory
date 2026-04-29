using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using MediatR;
using OneOf;
using OneOf.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelUpdates
{
    public class TLevelUpdateResponse 
    {
        [SwaggerSchema(Description = "TotalCount is the total number of T Level updates available based on the pagination parameters and cut off date.")]
        public int totalCount { get; set; }
        [SwaggerSchema(Description = "PageNumber is the current page number of the T Level updates.")]
        public int pageNumber { get; set; }
        [SwaggerSchema(Description = "PageSize is the number of T Level updates per page. Maximum value is 100.")]
        public int pageSize { get; set; }
        [SwaggerSchema(Description = "CutOffDate is the date used to filter T Level updates. Only updates on or after this date are included in the response. Expected format: yyyy-MM-ddTHH:mm:ss or yyyy-MM-dd")]
        public DateTime cutOffDate { get; set; }
        [SwaggerSchema(Description = "The list of T Level updates based on the pagination parameters and cut off date.")]
        public IList<TLevelUpdatesViewModel> courses { get; set; }
    }
    public class TLevelUpdateRequest : IRequest<OneOf<NotFound, TLevelUpdateResponse>>
    {
        [SwaggerSchema(Description = "CutOffDate is used to get the T Level updates on or after the specified date. Expected format: yyyy-MM-ddTHH:mm:ss or yyyy-MM-dd")]
        public DateTime CutOffDate { get; set; }
        [SwaggerSchema(Description = "PageSize is used to specify the number of T Level updates to be returned in a single page. Maximum allowed value is 100.")]
        public int PageSize { get; set; }
        [SwaggerSchema(Description = "PageNumber is used to specify the page number of the T Level updates to be returned.")]
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
