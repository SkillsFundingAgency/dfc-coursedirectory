using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels.YourCourses
{
    public class CourseViewModel
    {
        public string Id { get; set; }
        public string QualificationTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrg { get; set; }
        public IList<CourseRunViewModel> CourseRuns { get; set; }
        public string Facet { get; set; }
    }
}
