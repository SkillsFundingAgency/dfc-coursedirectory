using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
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

        //public ProviderSearchResultItem Value { get; set; }

        //public ProviderSearchResult(
        //    ProviderSearchResultItem value)
        //{
        //    Throw.IfNull(value, nameof(value));
        //}

        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    yield return Value;
        //}
    }
}
