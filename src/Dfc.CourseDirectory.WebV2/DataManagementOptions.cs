using System;

namespace Dfc.CourseDirectory.WebV2
{
    public class DataManagementOptions
    {
        public TimeSpan ProcessedImmediatelyThreshold { get; set; } = TimeSpan.FromSeconds(5);
    }
}
