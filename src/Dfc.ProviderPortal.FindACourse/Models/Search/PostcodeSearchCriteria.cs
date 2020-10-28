
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Interfaces;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class PostcodeSearchCriteria : ISearchCriteria // IPostcodeSearchCriteria
    {
        public string search { get; set; }
        public string searchMode { get; set; }
        public int? top { get; set; }
        public string filter { get; set; }
        public IEnumerable<string> facets { get; set; }
        public bool count { get; set; }
    }
}
