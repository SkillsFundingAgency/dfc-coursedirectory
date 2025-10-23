using System;

namespace Dfc.CourseDirectory.Core
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
