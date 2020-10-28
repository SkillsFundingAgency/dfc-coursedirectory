
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Interfaces;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class CourseDetailResult : ICourseDetailResult
    {
        public Course Course { get; set; }
        public dynamic /*Provider*/ Provider { get; set; }
        public dynamic /*Venue*/ Venue { get; set; }
        public dynamic /*Qualification*/ Qualification { get; set; }
    }
}
