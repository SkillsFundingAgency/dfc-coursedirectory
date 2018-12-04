using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IPostCodeSearchResult
    {
        IEnumerable<PostCodeSearchResultItem> Value { get; }
    }
}