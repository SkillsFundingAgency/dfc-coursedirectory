using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class SearchFacet : ISearchFacet
    {
        public int Count { get; set; }
        public string Value { get; set; }
    }
}
