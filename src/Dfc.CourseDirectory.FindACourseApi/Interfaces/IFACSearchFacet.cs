
using System;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IFACSearchFacet
    {
        int? count { get; }
        dynamic value { get; }
    }
}
