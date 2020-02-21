using System;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
