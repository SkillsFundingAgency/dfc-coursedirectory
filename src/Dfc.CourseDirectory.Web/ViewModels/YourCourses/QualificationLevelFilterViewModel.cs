using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels.YourCourses
{
    public class QualificationLevelFilterViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Facet { get; set; }
        public bool IsSelected { get; set; }
    }
}
