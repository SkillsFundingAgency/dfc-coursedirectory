
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class FACSearchFacet : IFACSearchFacet
    {
        public int? count { get; }
        public dynamic value { get; }
    }
}
