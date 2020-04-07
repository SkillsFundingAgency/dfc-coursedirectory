using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    [Flags]
    public enum ApprenticeshipDeliveryMode
    {
        None = 0,
        EmployerAddress = 1,
        DayRelease = 2,
        BlockRelease = 4
    }
}
