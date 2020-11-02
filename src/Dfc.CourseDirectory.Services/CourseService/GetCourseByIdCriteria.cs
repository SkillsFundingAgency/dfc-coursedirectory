using System;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class GetCourseByIdCriteria : IGetCourseByIdCriteria
    {
        public Guid Id { get; }


        public GetCourseByIdCriteria(
            Guid id)
        {

            Throw.IfNullGuid(id, nameof(id));

            Id = id;
        }
    }
}
