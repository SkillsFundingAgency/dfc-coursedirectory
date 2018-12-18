using Dfc.CourseDirectory.Models.Models.Providers;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.Interfaces.ProviderService
{
    public interface IProviderSearchResult
    {
        IEnumerable<Provider> Value { get; }
    }
}
