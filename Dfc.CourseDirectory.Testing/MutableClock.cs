using System;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.Testing
{
    public class MutableClock : IClock
    {
        // Completely arbitrary..
        public static readonly DateTime Start = new DateTime(2020, 2, 6, 11, 13, 3, DateTimeKind.Utc);

        public DateTime UtcNow { get; set; } = Start;
    }
}
