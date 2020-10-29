
using System;
using System.Linq;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class Region //: IRegion
    {
        public enum Regions
        {
            E12000001,
            E12000002,
            E12000003,
            E12000004,
            E12000005,
            E12000006,
            E12000007,
            E12000008,
            E12000009
        }

        public static IEnumerable<string> RegionList()
        {
            return Enum.GetValues(typeof(Regions)).Cast<string>();
        }
    }
}
