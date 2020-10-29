
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IFACSearchFacets
    {
        IEnumerable<FACSearchFacet> NotionalNVQLevelv2 { get; }
        //string NotionalNVQLevelv2ODataType { get; }
        IEnumerable<FACSearchFacet> ProviderName { get; }
        //string ProviderNameODataType { get; }
        IEnumerable<FACSearchFacet> Region { get; }
        //string RegionODataType { get; }
    }
}
