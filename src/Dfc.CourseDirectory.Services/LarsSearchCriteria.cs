using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchCriteria : ValueObject<LarsSearchCriteria>, ILarsSearchCriteria
    {
        public string Search { get; }
        public int Top { get; }
        public int Skip { get; }
        public bool Count => true;
        public string Filter { get; }
        public IEnumerable<LarsSearchFacet> Facets { get; }

        public LarsSearchCriteria(
            string search,
            int top,
            int skip,
            string filter = null,
            IEnumerable<LarsSearchFacet> facets = null)
        {
            //Throw.IfNullOrWhiteSpace(search, nameof(search));
            Throw.IfLessThan(1, top, nameof(top));
            Throw.IfLessThan(0, skip, nameof(skip));

            Search = search;
            Top = top;
            Skip = skip;
            Filter = filter;
            Facets = facets;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Search;
            yield return Top;
            yield return Skip;
            yield return Count;
            yield return Filter;
            yield return Facets;
        }
    }
}