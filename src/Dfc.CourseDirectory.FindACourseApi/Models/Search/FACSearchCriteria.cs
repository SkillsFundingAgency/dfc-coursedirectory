
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class FACSearchCriteria : IFACSearchCriteria
    {
        public string scoringProfile { get; set; }
        public string search { get; set; }
        public string searchMode { get; set; }
        public int? top { get; set; }
        public int? skip { get; set; }
        public string filter { get; set; }
        public IEnumerable<string> facets { get; set; }
        public bool count { get; set; }
        public string orderby { get; set; }
    }
}
