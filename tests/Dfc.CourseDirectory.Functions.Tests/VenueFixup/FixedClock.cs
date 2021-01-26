using System;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.Functions.Tests.VenueFixup
{
    /// <summary>
    /// Returns same time through life of object to test in/out
    /// </summary>
    public class FixedClock : IClock
    {
        public DateTime UtcNow { get; }

        public FixedClock()
        {
            UtcNow = DateTime.UtcNow;
        }
    }
}
