using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class SearchFacet : ValueObject<SearchFacet>, ISearchFacet
    {
        public int Count { get; }
        public string Value { get; }

        public SearchFacet(
            int count,
            string value)
        {
            Throw.IfLessThan(0, count, nameof(count));
            Throw.IfNullOrWhiteSpace(value, nameof(value));

            Count = count;
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Count;
            yield return Value;
        }
    }
}