using System;

namespace Dfc.CourseDirectory.Models.Interfaces.Onspd
{
    public interface IOnspd
    {
        string pcd { get; }
        string pcd2 { get; }
        string pcds { get; }
        string dointr { get; }
        string doterm { get; }
        string oscty { get; }
        string oslaua { get; }
        string osward { get; }
        string prsh { get; }
        string ctry { get; }
        string rgn { get; }
        decimal lat { get; }
        decimal @long { get; }
        string Parish { get; }
        string LocalAuthority { get; }
        string Region { get; }
        string County { get; }
        string Country { get; }
        DateTime updated { get; }
    }
}
