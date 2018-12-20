using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchFacets
    {
        IEnumerable<SearchFacet> AwardOrgCode { get; }
        string AwardOrgCodeODataType { get; }
        IEnumerable<SearchFacet> NotionalNVQLevelv2 { get; }
        string NotionalNVQLevelv2ODataType { get; }
    }
}