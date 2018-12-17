using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.ProviderService
{
    public class ProviderSearchResult : ValueObject<ProviderSearchResult>, IProviderSearchResult
    {
        public IEnumerable<Provider> Value { get; set; }

        public ProviderSearchResult(
            IEnumerable<Provider> value)
        {
            Throw.IfNull(value, nameof(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
