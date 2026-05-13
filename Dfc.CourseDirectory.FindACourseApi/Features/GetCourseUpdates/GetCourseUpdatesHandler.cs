using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourses;
using MediatR;
using OneOf;
using OneOf.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace Dfc.CourseDirectory.FindACourseApi.Features.GetCourseUpdates
{
    public class CourseUpdateResponse 
    {
        [SwaggerSchema(Description = "TotalCount is the total number of course updates available based on the CutOffDate filter.")]
        public int totalCount { get; set; }
        [SwaggerSchema(Description = "PageNumber is used to specify the page number of the course updates to be returned.")]
        public int pageNumber { get; set; }
        [SwaggerSchema(Description = "PageSize is used to specify the number of course updates to be returned in a single page.")]  
        public int pageSize { get; set; }
        [SwaggerSchema(Description = "CutOffDate is used to get the course updates on or after the specified date. Expected format: yyyy-MM-ddTHH:mm:ss or yyyy-MM-dd")]
        public DateTime cutOffDate { get; set; }
        [SwaggerSchema(Description = "Courses is a list of course updates that match the CutOffDate filter and pagination parameters.")]
        public IList<CourseUpdatesViewModel> courses { get; set; }
    }
    public class CourseUpdateRequest : IRequest<OneOf<NotFound, CourseUpdateResponse>>
    {
        [SwaggerSchema(Description = "CutOffDate is used to get the course updates on or after the specified date. Expected format: yyyy-MM-ddTHH:mm:ss or yyyy-MM-dd")]
        public DateTime CutOffDate { get; set; }
        [SwaggerSchema(Description = "PageSize is used to specify the number of course updates to be returned in a single page. Maximum allowed value is 100.")]
        public int PageSize { get; set; }
        [SwaggerSchema(Description = "PageNumber is used to specify the page number of the course updates to be returned.")]
        public int PageNumber { get; set; }
    }

    public class Handler : IRequestHandler<CourseUpdateRequest, OneOf<NotFound, CourseUpdateResponse>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher ?? throw new ArgumentNullException(nameof(sqlQueryDispatcher));
        }

        public async Task<OneOf<NotFound, CourseUpdateResponse>> Handle(CourseUpdateRequest request, CancellationToken cancellationToken)
        {
            var listOfCourses = await _sqlQueryDispatcher.ExecuteQuery(new Core.DataStore.Sql.Queries.GetCourseUpdates() { CutOffDate = request.CutOffDate, PageNumber = request.PageNumber, PageSize = request.PageSize });
            var response = new CourseUpdateResponse()
            {
                totalCount = listOfCourses.CourseCount,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
                cutOffDate = request.CutOffDate,
                courses = listOfCourses.Courses.Select(c => new CourseUpdatesViewModel()
                {
                    UpdateType = ConvertToEnumObj<UpdateType,GetCourses.UpdateType>(c.UpdateType),                    
                    Id = c.Id,
                    CourseRunStatus = ConvertToEnumObj<CourseStatus, CourseRunStatus>(c.CourseRunStatus),
                    ContactType = c.ContactType,
                    CreatedOn = c.CreatedOn,
                    UpdatedOn = c.UpdatedOn,
                    CourseUpdatedOn = c.CourseUpdatedOn,
                    VenueUpdatedOn = c.VenueUpdatedOn,
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    CourseType = c.CourseType,
                    SectorDescription = c.SectorDescription,
                    SectorCode = c.SectorCode,
                    SectorSubjectArea = c.SectorSubjectArea,
                    EducationLevel = ConvertToEnumObj<EducationLevel, CourseEducationLevel>(c.EducationLevel),
                    AwardingBody = c.AwardingBody,
                    DeliveryMode = ConvertToEnumObj<CourseDeliveryMode,DeliveryMode>(c.DeliveryMode) ,
                    FlexibleStartDate = c.FlexibleStartDate,
                    StartDate = c.StartDate,
                    CourseWebsite = c.CourseWebsite,
                    Cost = c.Cost,
                    CostDescription = c.CostDescription,
                    DurationUnit = ConvertToEnumObj<CourseDurationUnit, DurationUnit>(c.DurationUnit),
                    DurationValue = c.DurationValue,
                    StudyMode = ConvertToEnumObj<CourseStudyMode, StudyMode>(c.StudyMode),
                    AttendancePattern = ConvertToEnumObj<CourseAttendancePattern, AttendancePattern>(c.AttendancePattern), 
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
        public enum UpdateType
        {
            [Description("Newly Added Course")]
            NewlyAddedCourse = 1,
            [Description("Updated Course")]
            UpdatedCourse = 2,
            [Description("Deleted Course")]
            DeletedCourse = 3
        }
        
        private static V ConvertToEnumObj<T, V>(int? value) where V : IEnumObj, new()
        {
            Type enumType = typeof(T);
            if (value.HasValue && Enum.IsDefined(enumType, value))
            {
                var description = Enum.GetName(enumType, value);
                return new V() { Value = value, Description = description };
            }
            return default;
        }
    }
}
