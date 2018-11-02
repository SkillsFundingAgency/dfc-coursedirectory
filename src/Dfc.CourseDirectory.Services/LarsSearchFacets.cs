using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchFacets : ILarsSearchFacets
    {
        public string NotionalNVQLevelv2ODataType { get; set; }

        public string AwardOrgCodeODataType { get; set; }

        public IEnumerable<SearchFacet> NotionalNVQLevelv2 { get; set; }

        public IEnumerable<SearchFacet> AwardOrgCode { get; set; }
    }
}
