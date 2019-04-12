using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class ZCodeSearchCriteria : ValueObject<ZCodeSearchCriteria>, IZCodeSearchCriteria
    {
        public string search { get; }

        public string searchfields { get; }

        public int Top { get; }
        public int Skip { get; }
        public bool Count => true;
        public string Filter { get; }
        public IEnumerable<LarsSearchFacet> Facets { get; }

        public ZCodeSearchCriteria(
            string Search,
             string SearchFields,
            int top,
            int skip,
            string filter = null,
            IEnumerable<LarsSearchFacet> facets = null)
        {
            Throw.IfNullOrWhiteSpace(Search, nameof(Search));
            Throw.IfNullOrWhiteSpace(SearchFields, nameof(SearchFields));
            Throw.IfLessThan(1, top, nameof(top));
            Throw.IfLessThan(0, skip, nameof(skip));

            search = Search;
            searchfields = SearchFields;
            Top = top;
            Skip = skip;
            Filter = filter;
            Facets = facets;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return search;
            yield return searchfields;
            yield return Top;
            yield return Skip;
            yield return Count;
            yield return Filter;
            yield return Facets;
        }
    }
}