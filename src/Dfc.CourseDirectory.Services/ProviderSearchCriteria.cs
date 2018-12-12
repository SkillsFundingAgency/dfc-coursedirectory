using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
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
