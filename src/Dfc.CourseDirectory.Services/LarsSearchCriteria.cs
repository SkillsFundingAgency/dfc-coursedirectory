using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchCriteria : ValueObject<LarsSearchCriteria>, ILarsSearchCriteria
    {
        public string Search { get; }
        public bool Count { get; }
        public string Filter { get; }
        public IEnumerable<LarsSearchFacet> Facets { get; }

        public LarsSearchCriteria(
            string search,
            bool count = false,
            string filter = null,
            IEnumerable<LarsSearchFacet> facets = null)
        {
            Throw.IfNullOrWhiteSpace(search, nameof(search));

            Search = search;
            Count = count;
            Filter = filter;
            Facets = facets;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Search;
            yield return Count;
            yield return Filter;
            yield return Facets;
        }
    }
}