using System;
using Dfc.CourseDirectory.Models.Interfaces.Onspd;

namespace Dfc.CourseDirectory.Models.Models.Onspd
{
    public class Onspd : IOnspd
    {
        public string pcd { get; set; }
        public string pcd2 { get; set; }
        public string pcds { get; set; }
        public string dointr { get; set; }
        public string doterm { get; set; }
        public string oscty { get; set; }
        public string oslaua { get; set; }
        public string osward { get; set; }
        public string prsh { get; set; }
        public string ctry { get; set; }
        public string rgn { get; set; }
        public decimal lat { get; set; }
        public decimal @long { get; set; }
        public string Parish { get; set; }
        public string LocalAuthority { get; set; }
        public string Region { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public DateTime updated { get; set; }
    }
}
