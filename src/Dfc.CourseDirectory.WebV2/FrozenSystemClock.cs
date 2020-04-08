using System;

namespace Dfc.CourseDirectory.WebV2
{
    public class FrozenSystemClock : IClock
    {
        public DateTime UtcNow { get; } = DateTime.UtcNow;
    }
}
