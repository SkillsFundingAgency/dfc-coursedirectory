using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchResult : ILarsSearchResult
    {
        public string ODataContext { get; set; }

        public int? ODataCount { get; set; }

        public LarsSearchFacets SearchFacets { get; set; }

        public IEnumerable<LarsSearchResultItem> Value { get; set; }
    }
}
