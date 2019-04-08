using Dfc.CourseDirectory.Services.Enums;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IZCodeSearchCriteria
    {
        string search { get; }

        string searchfields { get; }
        string Filter { get; }
        IEnumerable<LarsSearchFacet> Facets { get; }
        bool Count { get; }
    }
}