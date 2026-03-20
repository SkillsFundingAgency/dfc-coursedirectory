using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetCourses
{
    public class CourseResponse 
    {
        public int totalCourseCount { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public IList<CourseListViewModel> courses { get; set; }
    }
    public class CourseRequest : IRequest<OneOf<NotFound, CourseResponse>>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class Handler : IRequestHandler<CourseRequest, OneOf<NotFound, CourseResponse>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));
        }

        public async Task<OneOf<NotFound, CourseResponse>> Handle(CourseRequest request, CancellationToken cancellationToken)
        {
            var listOfCourses = await _sqlQueryDispatcher.ExecuteQuery(new GetCourseList() { PageNumber = request.PageNumber, PageSize = request.PageSize });
            var response = new CourseResponse()
            {
                totalCourseCount = listOfCourses.CourseCount,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
                courses = listOfCourses.Courses.Select(c => new CourseListViewModel()
                {
                    CourseId = c.CourseId,
                    Id = c.Id,
                    CourseName = c.CourseName,
                    CourseType = c.CourseType,
                    SectorDescription = c.SectorDescription,
                    EducationLevel = ConvertToEnumObj<EducationLevel>(c.EducationLevel),
                    AwardingBody = c.AwardingBody,
                    DeliveryMode = ConvertToEnumObj<CourseDeliveryMode>(c.DeliveryMode) ,
                    FlexibleStartDate = c.FlexibleStartDate,
                    StartDate = c.StartDate,
                    CourseWebsite = c.CourseWebsite,
                    Cost = c.Cost,
                    CostDescription = c.CostDescription,
                    DurationUnit = ConvertToEnumObj<CourseDurationUnit>(c.DurationUnit),
                    DurationValue = c.DurationValue,
                    StudyMode = ConvertToEnumObj<CourseStudyMode>(c.StudyMode),
                    AttendancePattern = ConvertToEnumObj<CourseAttendancePattern>(c.AttendancePattern), 
                    National = c.National,
                    Region = c.Region,
                    ParentRegion = c.ParentRegion,
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
                    LearnAimRef = c.LearnAimRef,
                    LearnAimRefTitle = c.LearnAimRefTitle,
                    QualificationLevel = c.QualificationLevel,
                    AwardingOrganisation = c.AwardingOrganisation
                }).ToList()
            };
            return response;
        }
        private static EnumObj ConvertToEnumObj<T>(int? value)
        {
            Type enumType = typeof(T);
            if (value.HasValue && Enum.IsDefined(enumType, value))
            {
                var description = Enum.GetName(enumType, value);
                return new EnumObj() { Value = value, Description = description };
            }
            return null;
        }
    }
}
