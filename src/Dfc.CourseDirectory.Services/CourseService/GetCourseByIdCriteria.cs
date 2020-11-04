using System;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class GetCourseByIdCriteria
    {
        public Guid Id { get; }

        public GetCourseByIdCriteria(Guid id)
        {
            Id = id;
        }
    }
}
