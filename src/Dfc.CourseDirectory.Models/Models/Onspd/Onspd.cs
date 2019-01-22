using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Onspd;

namespace Dfc.CourseDirectory.Models.Models.Onspd
{
    public class Onspd : ValueObject<Onspd>, IOnspd
    {
        public string pcd { get; }
        public string pcd2 { get; }
        public string pcds { get; }
        public string dointr { get; }
        public string doterm { get; }
        public string oscty { get; }
        public string oslaua { get; }
        public string osward { get; }
        public string prsh { get; }
        public string ctry { get; }
        public string rgn { get; }
        public decimal lat { get; }
        public decimal @long { get; }
        public string Parish { get; }
        public string LocalAuthority { get; }
        public string Region { get; }
        public string County { get; }
        public string Country { get; }
        public DateTime updated { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return pcd;
            yield return pcd2;
            yield return pcds;
            yield return dointr;
            yield return doterm;
            yield return oscty;
            yield return oslaua;
            yield return osward;
            yield return prsh;
            yield return ctry;
            yield return rgn;
            yield return lat;
            yield return @long;
            yield return Parish;
            yield return LocalAuthority;
            yield return Region;
            yield return County;
            yield return Country;
            yield return updated;
        }
    }
}
