using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.ProviderService
{
    public class ProviderSearchCriteria : ValueObject<ProviderSearchCriteria>, IProviderSearchCriteria
    {
        public string Search { get; }

        public ProviderSearchCriteria(
            string search)
        {
            Throw.IfNullOrWhiteSpace(search, nameof(search));

            Search = search;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Search;
        }
    }
}
