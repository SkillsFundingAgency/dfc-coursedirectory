using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class PostCodeSearchResult : ValueObject<PostCodeSearchResult>, IPostCodeSearchResult
    {
        public IEnumerable<PostCodeSearchResultItem> Value { get; set; }

        public PostCodeSearchResult(
            IEnumerable<PostCodeSearchResultItem> value)
        {
            Throw.IfNull(value, nameof(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}