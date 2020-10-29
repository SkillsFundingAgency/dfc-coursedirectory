using System;

namespace Dfc.CourseDirectory.FindACourseApi.ApiModels
{
    public class CourseRunDetailRequest
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
    }
}
