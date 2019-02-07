using System;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class GetCourseByIdCriteria : ValueObject<GetCourseByIdCriteria>, IGetCourseByIdCriteria
    {
        public Guid Id { get; }


        public GetCourseByIdCriteria(
            Guid id)
        {

            Throw.IfNullGuid(id, nameof(id));

            Id = id;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}