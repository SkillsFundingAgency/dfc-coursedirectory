using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchResult : ValueObject<LarsSearchResult>, ILarsSearchResult
    {
        public string ODataContext { get; }
        public int? ODataCount { get; }
        public LarsSearchFacets SearchFacets { get; }
        public IEnumerable<LarsSearchResultItem> Value { get; }

        public LarsSearchResult(
            string oDataContext,
            int? oDataCount,
            LarsSearchFacets larsSearchFacets,
            IEnumerable<LarsSearchResultItem> value)
        {
            Throw.IfNullOrWhiteSpace(oDataContext, nameof(oDataContext));
            if (oDataCount.HasValue) Throw.IfLessThan(0, oDataCount.Value, nameof(oDataCount));
            Throw.IfNull(larsSearchFacets, nameof(larsSearchFacets));
            Throw.IfNull(value, nameof(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ODataContext;
            yield return ODataCount;
            yield return SearchFacets;
            yield return Value;
        }
    }
}