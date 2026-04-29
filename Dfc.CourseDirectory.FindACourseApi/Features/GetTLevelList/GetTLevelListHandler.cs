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
using Swashbuckle.AspNetCore.Annotations;

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelList
{
    public class TLevelResponse 
    {
        [SwaggerSchema(Description = "TotalCount is the total number of T Levels available in the system.")]
        public int totalCount { get; set; }
        [SwaggerSchema(Description = "PageNumber is used to specify the page number of the T Levels to be returned.")]
        public int pageNumber { get; set; }
        [SwaggerSchema(Description = "PageSize is used to specify the number of T Levels to be returned in a single page. Maximum allowed value is 100.")]
        public int pageSize { get; set; }
        [SwaggerSchema(Description = "Courses is a list of T Levels returned based on the specified page number and page size.")]
        public IList<TLevelListViewModel> courses { get; set; }
    }
    public class TLevelRequest : IRequest<OneOf<NotFound, TLevelResponse>>
    {
        [SwaggerSchema(Description = "PageSize is used to specify the number of T Levels to be returned in a single page. Maximum allowed value is 100.")]
        public int PageSize { get; set; }
        [SwaggerSchema(Description = "PageNumber is used to specify the page number of the T Levels to be returned.")]
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
                totalCount = listOfTLevels.TLevelsCount,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
                courses = listOfTLevels.TLevels.Select(c => new TLevelListViewModel()
                {
                    TLevelId = c.TLevelId,
                    StartDate = c.StartDate,
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
