using System;

namespace Dfc.CourseDirectory.Core
{
    public class FrozenSystemClock : IClock
    {
        public DateTime UtcNow { get; } = DateTime.UtcNow;
    }
}
