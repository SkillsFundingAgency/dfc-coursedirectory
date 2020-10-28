using System;

namespace Dfc.ProviderPortal.FindACourse.ApiModels
{
    public class CourseRunDetailRequest
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
    }
}
