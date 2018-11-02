using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchCriteria : ILarsSearchCriteria
    {
        public string Search { get; }
        public string Filter { get; }
        public IEnumerable<LarsSearchFacet> Facets { get; }
        public bool Count { get; }

        public LarsSearchCriteria(
            string search,
            string filter,
            IEnumerable<LarsSearchFacet> facets,
            bool count)
        {
            if (string.IsNullOrWhiteSpace(search)) throw new ArgumentException("Cannot be null, empty or whitespace only.", nameof(search));

            Search = search;
            Filter = filter;
            Facets = facets;
            Count = count;
        }
    }
}
