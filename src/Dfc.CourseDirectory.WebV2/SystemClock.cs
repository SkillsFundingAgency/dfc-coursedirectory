using System;

namespace Dfc.CourseDirectory.WebV2
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
