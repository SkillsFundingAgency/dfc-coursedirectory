using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchResult
    {
        string ODataContext { get; }
        int ODataCount { get; }
        ILarsSearchFacets SearchFacets { get; }
        IEnumerable<ILarsSearchResultItem> Value { get; }
    }
}

