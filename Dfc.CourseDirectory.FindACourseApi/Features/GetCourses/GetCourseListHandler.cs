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
        public IList<CourseListViewModel> Courses { get; set; }
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
                Courses = listOfCourses.Courses.Select(c => new CourseListViewModel()
                {
                    CourseName = c.CourseName,
                    CourseType = c.CourseType,
                    SectorDescription = c.SectorDescription,
                    EducationLevel = CovertToEnumObj<EducationLevel>(c.EducationLevel),
                    AwardingBody = c.AwardingBody,
                    DeliveryMode = CovertToEnumObj<CourseDeliveryMode>(c.DeliveryMode) ,
                    FlexibleStartDate = c.FlexibleStartDate,
                    StartDate = c.StartDate,
                    CourseWebsite = c.CourseWebsite,
                    Cost = c.Cost,
                    CostDescription = c.CostDescription,
                    DurationUnit = CovertToEnumObj<CourseDurationUnit>(c.DurationUnit),
                    DurationValue = c.DurationValue,
                    StudyMode = CovertToEnumObj<CourseStudyMode>(c.StudyMode),
                    AttendancePattern = CovertToEnumObj<CourseAttendancePattern>(c.AttendancePattern), 
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
                }).ToList()
            };
            return response;
        }
        private static EnumObj CovertToEnumObj<T>(object value)
        {
            Type enumType = typeof(T);
            var description = string.Empty;
            if (Enum.IsDefined(enumType, value))
                description = Enum.GetName(enumType, value);
            return new EnumObj() { Value = value, Description = description };
        }
    }
}
