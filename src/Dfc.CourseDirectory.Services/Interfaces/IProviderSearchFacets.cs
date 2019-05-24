
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IProviderSearchFacets
    {
        IEnumerable<SearchFacet> Town { get; }
        string TownODataType { get; }
        //IEnumerable<SearchFacet> Region { get; }
        //string RegionODataType { get; }
    }
}