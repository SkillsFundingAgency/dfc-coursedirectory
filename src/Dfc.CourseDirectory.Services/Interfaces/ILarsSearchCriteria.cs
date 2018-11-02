using Dfc.CourseDirectory.Services.Enums;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchCriteria
    {
        string Search { get; }
        string Filter { get; }
        IEnumerable<LarsSearchFacet> Facets { get; }
        bool Count { get; }
    }
}