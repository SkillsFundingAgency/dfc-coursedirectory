using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchFacets
    {
        string NotionalNVQLevelv2ODataType { get; }
        string AwardOrgCodeODataType { get; }
        IEnumerable<ISearchFacet> NotionalNVQLevelv2 { get; }
        IEnumerable<ISearchFacet> AwardOrgCode { get; }
    }
}