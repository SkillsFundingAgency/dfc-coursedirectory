using Dfc.CourseDirectory.Models.Models.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IProviderSearchResult
    {
        IEnumerable<Provider> Value { get; }
        //ProviderSearchResultItem Value { get; }
    }
}
