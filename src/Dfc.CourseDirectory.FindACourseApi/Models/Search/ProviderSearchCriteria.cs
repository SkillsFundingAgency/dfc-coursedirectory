
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class ProviderSearchCriteria : ISearchCriteria // IProviderSearchCriteria
    {
        public string search { get; set; }
        public string searchMode { get; set; }
        public int? top { get; set; }
        public string filter { get; set; }
        public IEnumerable<string> facets { get; set; }
        public bool count { get; set; }
    }
}
